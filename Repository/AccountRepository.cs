using AutoMapper;
using MaxemusAPI.Data;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using static MaxemusAPI.Common.GlobalVariables;
using System.Net;

namespace MaxemusAPI.Repository
{
    public class AccountRepository : IAccountRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _contentRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public AccountRepository(ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, IContentRepository contentRepository, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _response = new();
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _contentRepository = contentRepository;
        }

        public bool IsUniqueUser(string email, string phoneNumber)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => (x.Email.ToLower() == email.ToLower()) || (x.PhoneNumber == phoneNumber));
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<ApplicationUser> IsValidUser(string EmailOrPhone)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => (x.Email.ToLower() == EmailOrPhone.ToLower()) || (x.PhoneNumber == EmailOrPhone));
            if (user == null)
            {
                return user;
            }
            return new ApplicationUser();
        }
        public bool IsUniqueEmail(string email)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => (x.Email.ToLower() == email.ToLower()));
            if (user == null)
            {
                return true;
            }
            return false;
        }
        public bool IsUniquePhone(string phoneNumber)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => (x.PhoneNumber == phoneNumber));
            if (user == null)
            {
                return true;
            }
            return false;
        }
        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _context.ApplicationUsers
                .FirstOrDefault(u => (u.Email.ToLower() == loginRequestDTO.emailOrPhone.ToLower()) || u.PhoneNumber.ToLower() == loginRequestDTO.emailOrPhone.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.password);


            if (user == null || isValid == false)
            {
                return new LoginResponseDTO();
            }

            //if user was found generate JWT Token
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim("SecurityStamp", user.SecurityStamp),
                    // new Claim(ClaimTypes.Anonymous,user.SecurityStamp)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                token = tokenHandler.WriteToken(token),
            };
            _mapper.Map(user, loginResponseDTO);

            var userdetail = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userdetail == null)
            {
                return new LoginResponseDTO();
            }
            loginResponseDTO.role = roles[0];

            loginResponseDTO.gender = userdetail.Gender;
            loginResponseDTO.dialCode = userdetail.DialCode;

            if (roles.FirstOrDefault() == "Distributor")
            {
                var distributor = _context.DistributorDetail.Where(d => d.UserId == user.Id).FirstOrDefault();

                loginResponseDTO.status = distributor.Status;
                loginResponseDTO.distributorId = distributor.DistributorId;
            }

            return loginResponseDTO;
        }
        public async Task<LoginResponseDTO> Register(RegisterationRequestDTO registerationRequestDTO)
        {
            ApplicationUser user = new()
            {
                Email = registerationRequestDTO.email,
                UserName = registerationRequestDTO.email,
                NormalizedEmail = registerationRequestDTO.email.ToUpper(),
                FirstName = registerationRequestDTO.firstName,
                LastName = registerationRequestDTO.lastName,
                PhoneNumber = registerationRequestDTO.phoneNumber,
                CountryId = (registerationRequestDTO.countryId == 0 ? null : registerationRequestDTO.countryId),
                StateId = (registerationRequestDTO.stateId == 0 ? null : registerationRequestDTO.stateId),
                City = registerationRequestDTO.City,
                PostalCode = registerationRequestDTO.PostalCode,
                Gender = registerationRequestDTO.gender,
                DialCode = registerationRequestDTO.dialCode,
                DeviceType = registerationRequestDTO.deviceType
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationRequestDTO.password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(registerationRequestDTO.role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.role));
                    }

                    await _userManager.AddToRoleAsync(user, registerationRequestDTO.role);

                    var userToReturn = await _context.ApplicationUsers
                        .Where(u => u.Email.ToLower() == registerationRequestDTO.email.ToLower()).FirstOrDefaultAsync();
                    if (registerationRequestDTO.role == Role.Distributor.ToString())
                    {
                        var distributor = new DistributorDetail
                        {
                            Name = " ",
                            RegistrationNumber = " ",
                            Status = Status.Incomplete.ToString(),
                            UserId = userToReturn.Id,
                            DistributorCode = CommonMethod.RandomString(6),
                        };

                        await _context.AddAsync(distributor);
                        var d = await _context.SaveChangesAsync();
                    }
                    if (registerationRequestDTO.role == Role.Dealer.ToString())
                    {
                        var dealerDetail = new DealerDetail
                        {
                            DistributorCode = registerationRequestDTO.distributorCode,
                            DistributorId = _context.DistributorDetail.Where(u => u.DistributorCode == registerationRequestDTO.distributorCode).FirstOrDefault().DistributorId,
                            Status = Status.Pending.ToString(),
                            UserId = userToReturn.Id,
                        };

                        await _context.AddAsync(dealerDetail);
                        await _context.SaveChangesAsync();
                    }

                    LoginRequestDTO loginRequestDTO = new LoginRequestDTO();
                    loginRequestDTO.emailOrPhone = registerationRequestDTO.email;
                    loginRequestDTO.password = registerationRequestDTO.password;
                    LoginResponseDTO loginResponseDTO = await Login(loginRequestDTO);

                    return loginResponseDTO;
                }
            }
            catch (Exception e)
            {

            }

            return new LoginResponseDTO();
        }
    }
}
