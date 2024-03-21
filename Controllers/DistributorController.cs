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

        #region AddorUpdateAddress
        /// <summary>
        ///  AddOrUpdateAddress for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateAddress")]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddOrUpdateAddress([FromBody] DistributorAddressDTO model)
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

            var distributorId = await _context.DistributorDetail
                .Where(u => u.UserId == currentUserId)
                .Select(u => u.DistributorId)
                .FirstOrDefaultAsync();

            if (distributorId == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "distributor.";
                return Ok(_response);
            }

            if (model.AddressId > 0)
            {
                var existingAddress = await _context.DistributorAddress
                    .FirstOrDefaultAsync(u => u.AddressId == model.AddressId);

                if (existingAddress != null)
                {
                    existingAddress.AddressType = model.AddressType;
                    existingAddress.CountryId = model.CountryId;
                    existingAddress.StateId = model.StateId;
                    existingAddress.City = model.City;
                    existingAddress.HouseNoOrBuildingName = model.HouseNoOrBuildingName;
                    existingAddress.StreetAddress = model.StreetAddress;
                    existingAddress.Landmark = model.Landmark;
                    existingAddress.PostalCode = model.PostalCode;
                    existingAddress.PhoneNumber = model.PhoneNumber;

                    _context.Update(existingAddress);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<DistributorAddressResponseDTO>(existingAddress);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Address updated successfully.";
                    return Ok(_response);
                }
            }

            var address = await _context.DistributorAddress
                .FirstOrDefaultAsync(u => u.DistributorId == distributorId);

            if (address != null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Address already exists for the distributor.";
                return Ok(_response);
            }

            var distributorAddress = new DistributorAddress
            {
                DistributorId = distributorId,
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

            var newResponse = _mapper.Map<DistributorAddressResponseDTO>(distributorAddress);
            newResponse.AddressId = distributorAddress.AddressId;

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = newResponse;
            _response.Messages = "Address added successfully.";
            return Ok(_response);



            return Ok(_response);

        }
        #endregion

        #region AddOrUpdateBusinessProfile
        /// <summary>
        ///  AddOrUpdateBusinessProfile for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateBusinessProfile")]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddOrUpdateBusinessProfile([FromBody] DistributorDetailDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }

            var existingUser = await _context.ApplicationUsers.FindAsync(currentUserId);
            if (existingUser == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any user.";
                return Ok(_response);
            }

            if (model.DistributorId > 0)
            {
                var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId && u.UserId == currentUserId);
                if (distributorDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail not found.";
                    return Ok(_response);
                }

                distributorDetail.Name = existingUser.UserName;
                distributorDetail.RegistrationNumber = model.RegistrationNumber;
                //distributorDetail.AddressId = await _context.DistributorAddress.Where(u => u.DistributorId == model.DistributorId).Select(u => u.AddressId).FirstOrDefaultAsync();
                distributorDetail.Description = model.Description;
                distributorDetail.Image = model.Image;

                _context.Update(distributorDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                return Ok(_response);
            }

            if (model.DistributorId == 0)
            {
                var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (distributorDetail == null)
                {
                    distributorDetail = new DistributorDetail
                    {
                        UserId = currentUserId,
                        Name = existingUser.UserName,
                        //AddressId = await _context.DistributorAddress.Where(u => u.DistributorId == model.DistributorId).Select(u => u.AddressId).FirstOrDefaultAsync(),
                        RegistrationNumber = model.RegistrationNumber,
                        Description = model.Description,
                        Image = model.Image
                    };

                    _context.Add(distributorDetail);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Profile added successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail already exists.";
                    return Ok(_response);
                }
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
        [HttpPost]
        [Route("UpdatePersonalProfile")]
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

        #region SetProductStatus
        /// <summary>
        ///   Set distributor status [Pending = 0; Approved = 1; Rejected = 2]..
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("SetDistributorStatus")]
        public async Task<IActionResult> SetDistributorStatus([FromBody] SetDistributorStatusDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }
            var currentUserDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();
            if (currentUserDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            if (model.status != Convert.ToInt32(Status.Pending)
          && model.status != Convert.ToInt32(Status.Approved)
          && model.status != Convert.ToInt32(Status.Rejected))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please select a valid status.";
                return Ok(_response);
            }

            var distributor = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.DistributorId == model.distributorId);

            if (distributor == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            distributor.Status = model.status.ToString();

            _context.Update(distributor);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "distributor status updated successfully.";

            return Ok(_response);
        }

        #endregion
    }
}
