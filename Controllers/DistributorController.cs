using AutoMapper;
using MaxemusAPI.Data;
using MaxemusAPI.Helpers;
using MaxemusAPI.IRepository;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Models;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MaxemusAPI.Common.GlobalVariables;
using Google.Api.Gax;
using MaxemusAPI.Common;
using Twilio.Http;
using static Google.Apis.Requests.BatchRequest;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _contentRepository;
        private readonly IAccountRepository _userRepo;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IEmailManager _emailSender;
        private ITwilioManager _twilioManager;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DistributorController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, IContentRepository contentRepository, RoleManager<IdentityRole> roleManager, IEmailManager emailSender, ITwilioManager twilioManager)
        {
            _userRepo = userRepo;
            _response = new();
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _twilioManager = twilioManager;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _contentRepository = contentRepository;
            _hostingEnvironment = hostingEnvironment;
        }


        #region AddOrUpdateBusinessProfile
        /// <summary>
        ///  Add Or Update Profile for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddOrUpdateProfile")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddOrUpdateProfile([FromBody] DistributorDetailDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }

            if (AddressType.Individual.ToString() != model.AddressType
                && AddressType.Company.ToString() != model.AddressType
                && AddressType.Shipping.ToString() != model.AddressType
                && AddressType.Billing.ToString() != model.AddressType
               )
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter correct address type.";
                return Ok(_response);
            }

            var userProfileDetail = await _context.ApplicationUsers.FindAsync(currentUserId);
            if (userProfileDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "user.";
                return Ok(_response);
            }


            if (model.DistributorId > 0)
            {
                var distributorDetail = await _context.DistributorDetail
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId && u.DistributorId == model.DistributorId);
                if (distributorDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail not found.";
                    return Ok(_response);
                }

                var addressExists = await _context.DistributorAddress
                    .FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId);
                if (addressExists == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Address not found.";
                    return Ok(_response);
                }

                addressExists.DistributorId = model.DistributorId;
                addressExists.AddressType = model.AddressType;
                addressExists.CountryId = model.CountryId;
                addressExists.StateId = model.StateId;
                addressExists.City = model.City;
                addressExists.HouseNoOrBuildingName = model.HouseNoOrBuildingName;
                addressExists.StreetAddress = model.StreetAddress;
                addressExists.Landmark = model.Landmark;
                addressExists.PostalCode = model.PostalCode;
                addressExists.PhoneNumber = model.PhoneNumber;


                _context.Update(addressExists);
                await _context.SaveChangesAsync();

                distributorDetail.Name = userProfileDetail.FirstName + " " + userProfileDetail.LastName;
                distributorDetail.RegistrationNumber = model.RegistrationNumber;
                distributorDetail.Description = model.Description;

                _context.Update(distributorDetail);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorAddressResponseDTO>(distributorDetail);
                response.CreateDate = distributorDetail.CreateDate.ToString("dd-MM-yyyy");
                _mapper.Map(addressExists, response);


                var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.CountryId).FirstOrDefaultAsync();
                var distributorState = await _context.StateMaster.Where(u => u.StateId == response.StateId).FirstOrDefaultAsync();
                response.CountryName = distributorCountry.CountryName;
                response.StateName = distributorState.StateName;

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                _response.Data = response;
                return Ok(_response);
            }

            if (model.DistributorId == 0)
            {
                var distributorDetail = await _context.DistributorDetail
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (distributorDetail != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail already exists.";
                    return Ok(_response);
                }

                distributorDetail = new DistributorDetail
                {
                    UserId = currentUserId,
                    Name = userProfileDetail.FirstName + " " + userProfileDetail.LastName,
                    RegistrationNumber = model.RegistrationNumber,
                    Description = model.Description,
                };

                _context.Add(distributorDetail);
                await _context.SaveChangesAsync();

                var addressExists = await _context.DistributorAddress
                    .FirstOrDefaultAsync(u => u.DistributorId == distributorDetail.DistributorId);
                if (addressExists != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Address already exists for the distributor.";
                    return Ok(_response);
                }

                var distributorAddress = new DistributorAddress
                {
                    DistributorId = distributorDetail.DistributorId,
                    AddressType = model.AddressType,
                    CountryId = model.CountryId,
                    StateId = model.StateId,
                    City = model.City,
                    HouseNoOrBuildingName = model.HouseNoOrBuildingName,
                    StreetAddress = model.StreetAddress,
                    Landmark = model.Landmark,
                    PostalCode = model.PostalCode,
                    PhoneNumber = model.PhoneNumber
                };

                _context.Add(distributorAddress);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorAddressResponseDTO>(distributorDetail);
                response.CreateDate = distributorDetail.CreateDate.ToString("dd-MM-yyyy");
                _mapper.Map(distributorAddress, response);


                var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.CountryId).FirstOrDefaultAsync();
                var distributorState = await _context.StateMaster.Where(u => u.StateId == response.StateId).FirstOrDefaultAsync();
                response.CountryName = distributorCountry.CountryName;
                response.StateName = distributorState.StateName;

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile added successfully.";
                _response.Data = response;
                return Ok(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Profile added successfully.";
            return Ok(_response);
        }

        #endregion

        #region UpdateProfile
        /// <summary>
        ///  Update Personal profile.
        /// </summary>
        [HttpPost("UpdatePersonalProfile")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> UpdatePersonalProfile([FromBody] UserRequestDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }
            var userDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
            if (userDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }
            if (model.email.ToLower() != userDetail.Email.ToLower())
            {
                var userProfile = await _context.ApplicationUsers.Where(u => u.Email == model.email && u.Id != currentUserId).FirstOrDefaultAsync();
                if (userProfile != null)
                {
                    if (userProfile.Id != model.email)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Email already exists.";
                        return Ok(_response);
                    }
                }
            }
            if (model.phoneNumber.ToLower() != userDetail.PhoneNumber.ToLower())
            {
                var userProfile = await _context.ApplicationUsers.Where(u => u.PhoneNumber == model.phoneNumber && u.Id != currentUserId).FirstOrDefaultAsync();
                if (userProfile != null)
                {
                    if (userProfile.Id != model.email)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Phone number already exists.";
                        return Ok(_response);
                    }
                }
            }
            if (Gender.Male.ToString() != model.gender && Gender.Female.ToString() != model.gender && Gender.Others.ToString() != model.gender)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid gender.";
                return Ok(_response);
            }

            var mappedData = _mapper.Map(model, userDetail);
            _context.Update(userDetail);
            await _context.SaveChangesAsync();

            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(model, userProfileDetail);
            _context.ApplicationUsers.Update(updateProfile);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Profile updated successfully.";
            return Ok(_response);
        }
        #endregion


    }
}
