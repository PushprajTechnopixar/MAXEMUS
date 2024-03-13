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
            //if (roles[0] == "Vendor")
            //{
            //    loginResponseDTO.VendorId = userdetail.UserId;
            //    var shop = await _context.ShopDetail.Where(u => u.VendorId == userdetail.UserId).FirstOrDefaultAsync();
            //    if (shop != null)
            //    {
            //        loginResponseDTO.ShopName = shop.ShopName;
            //        loginResponseDTO.ShopImage = shop.ShopImage;
            //        loginResponseDTO.ShopId = shop.ShopId;
            //        loginResponseDTO.planType = 0;
            //        if (shop.DairyStatus == true && shop.GroceryStatus == true)
            //        {
            //            loginResponseDTO.planType = 3;
            //        }
            //        else if (shop.DairyStatus == true && shop.GroceryStatus != true)
            //        {
            //            loginResponseDTO.planType = 2;
            //        }
            //        else if (shop.DairyStatus == false && shop.GroceryStatus == true)
            //        {
            //            loginResponseDTO.planType = 1;
            //        }
            //    }
            //}
            loginResponseDTO.gender = userdetail.Gender;
            loginResponseDTO.dialCode = userdetail.DialCode;

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
                CountryId = registerationRequestDTO.countryId,
                StateId = registerationRequestDTO.stateId,
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

        public async Task<LoginResponseDTO> DistributorRegistration(DistributorDTO model)
        {
            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CountryId = model.CountryId,
                StateId = model.StateId,
                Gender = model.Gender,
                DialCode = model.DialCode,
                DeviceType = model.DeviceType
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Distributor"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Distributor"));
                    }

                    await _userManager.AddToRoleAsync(user, "Distributor");

                    // Add user detail
                    var distributorDetail = _mapper.Map<DistributorDetail>(model);

                    distributorDetail.UserId = user.Id;
                    distributorDetail.Name = user.FirstName + " " + user.LastName;
                   
                    _context.DistributorDetail.Add(distributorDetail);
                    await _context.SaveChangesAsync();

                    var userToReturn = await _context.ApplicationUsers
                        .Where(u => u.Email.ToLower() == model.Email.ToLower()).FirstOrDefaultAsync();
                    LoginRequestDTO loginRequestDTO = new LoginRequestDTO();
                    loginRequestDTO.emailOrPhone = model.Email;
                    loginRequestDTO.password = model.Password;
                    LoginResponseDTO loginResponseDTO = await Login(loginRequestDTO);

                    return loginResponseDTO;
                }
            }
            catch (Exception e)
            {

            }
            return new LoginResponseDTO();
        }

        public async Task<LoginResponseDTO> DealerRegistration(DealerDetailDTO model)
        {
            ApplicationUser user = new()
            {
                Email = model.email,
                UserName = model.email,
                NormalizedEmail = model.email.ToUpper(),
                FirstName = model.firstName,
                LastName = model.lastName,
                PhoneNumber = model.phoneNumber,
                CountryId = model.countryId,
                StateId = model.stateId,
                Gender = model.gender,
                DialCode = model.dialCode,
                DeviceType = model.deviceType
            };

            try
            {
                //var password = CommonMethod.GeneratePassword();
                var result = await _userManager.CreateAsync(user, model.password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Dealer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Dealer"));
                    }

                    await _userManager.AddToRoleAsync(user, "Dealer");

                    // Add user detail
                    var dealerDetail = _mapper.Map<DealerDetail>(model);
                    dealerDetail.UserId = user.Id;


                    _context.DealerDetail.Add(dealerDetail);
                    await _context.SaveChangesAsync();

                    var userToReturn = await _context.ApplicationUsers
                        .Where(u => u.Email.ToLower() == model.email.ToLower()).FirstOrDefaultAsync();
                    LoginRequestDTO loginRequestDTO = new LoginRequestDTO();
                    loginRequestDTO.emailOrPhone = model.email;
                    loginRequestDTO.password = model.password;
                    LoginResponseDTO loginResponseDTO = await Login(loginRequestDTO);

                    return loginResponseDTO;
                }
            }
            catch (Exception e)
            {

            }
            return new LoginResponseDTO();


        }

        //public async Task<Object> GetDistributorList(FilterationListDTO model)
        //{
        //    var distributorUsers = await _userManager.GetUsersInRoleAsync(Role.Distributor.ToString());
        //    if (distributorUsers.Count < 1)
        //    {
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = false;
        //        _response.Messages = "Record not found.";
        //        return _response;
        //    }

        //    List<AdminUserListDTO> distributorUserList = new List<AdminUserListDTO>();
        //    foreach (var item in distributorUsers)
        //    {
        //        var adminUserDetail = _userManager.FindByIdAsync(item.Id).GetAwaiter().GetResult();
        //        var adminUserProfileDetail = await _context.ApplicationUsers.FirstOrDefaultAsync(u => (u.Id == item.Id) && (u.IsDeleted == false));
        //        if (adminUserProfileDetail != null)
        //        {
        //            var mappedData = _mapper.Map<AdminUserListDTO>(item);
        //            mappedData.profilepic = adminUserProfileDetail.ProfilePic;
        //            mappedData.gender = adminUserProfileDetail.Gender;
        //            mappedData.modifyDate = adminUserProfileDetail.ModifyDate;
        //            distributorUserList.Add(mappedData);
        //        }
        //    }

        //    distributorUserList = distributorUserList.OrderByDescending(u => u.modifyDate).ToList();

        //    if (!string.IsNullOrEmpty(model.searchQuery))
        //    {
        //        distributorUserList = distributorUserList.Where(u => u.firstName.ToLower().Contains(model.searchQuery.ToLower())
        //        || u.email.ToLower().Contains(model.searchQuery.ToLower())
        //        ).ToList();
        //    }

        //    int count = distributorUserList.Count();
        //    int CurrentPage = model.pageNumber;
        //    int PageSize = model.pageSize;
        //    int TotalCount = count;
        //    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        //    var items = distributorUserList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        //    var previousPage = CurrentPage > 1 ? "Yes" : "No";
        //    var nextPage = CurrentPage < TotalPages ? "Yes" : "No";


        //    FilterationResponseModel<AdminUserListDTO> obj = new FilterationResponseModel<AdminUserListDTO>();
        //    obj.totalCount = TotalCount;
        //    obj.pageSize = PageSize;
        //    obj.currentPage = CurrentPage;
        //    obj.totalPages = TotalPages;
        //    obj.previousPage = previousPage;
        //    obj.nextPage = nextPage;
        //    obj.searchQuery = string.IsNullOrEmpty(model.searchQuery) ? "no parameter passed" : model.searchQuery;
        //    obj.dataList = items.ToList();

        //    if (obj == null)
        //    {
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = false;
        //        _response.Messages = "Error while adding.";
        //        return _response;
        //    }
        //    _response.StatusCode = HttpStatusCode.OK;
        //    _response.IsSuccess = true;
        //    _response.Data = obj;
        //    _response.Messages = "List shown successfully.";
        //    return _response;


        //}

    }
}
