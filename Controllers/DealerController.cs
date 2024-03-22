using AutoMapper;
using MaxemusAPI.Data;
using MaxemusAPI.Helpers;
using MaxemusAPI.IRepository;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Models;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MaxemusAPI.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using MaxemusAPI.Common;
using static MaxemusAPI.Common.GlobalVariables;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerController : ControllerBase
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

        public DealerController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
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

        #region AddOrUpdateAddress
        /// <summary>
        ///  AddOrUpdateProfile for Dealer.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddAddress")]
        [Authorize]
        public async Task<IActionResult> AddAddress([FromBody] DealerProfileDTO model)
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

            if (model.DealerId > 0)
            {
                var existingDealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.DealerId == model.DealerId && u.UserId == currentUserId);
                if (existingDealerDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Dealer profile not found.";
                    return Ok(_response);
                }

                existingDealerDetail.Address1 = model.Address1;
                existingDealerDetail.Address2 = model.Address2;

                _context.Update(existingDealerDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                return Ok(_response);
            }

            var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (dealerDetail != null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "User profile is already present.";
                return Ok(_response);
            }

            dealerDetail = new DealerDetail
            {
                UserId = currentUserId,
                Address1 = model.Address1,
                Address2 = model.Address2
            };

            if (model.DealerId == 0)
            {
                _context.Add(dealerDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile added successfully.";
                return Ok(_response);
            }



            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.Messages = "Invalid request.";
            return BadRequest(_response);

        }
        #endregion

        #region GetProfileDetail
        /// <summary>
        ///  Get profile.
        /// </summary>
        [HttpGet]
        [Route("GetProfileDetail")]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> GetProfileDetail()
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
            var dealerDetail = await _context.DealerDetail.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            var mappedData = _mapper.Map<DealerResponseDTO>(userDetail);
            _mapper.Map(dealerDetail, mappedData);
            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(userProfileDetail, mappedData);

            var userCountryDetail = await _context.CountryMaster.Where(u => u.CountryId == userProfileDetail.CountryId).FirstOrDefaultAsync();
            var userStateDetail = await _context.StateMaster.Where(u => u.StateId == userProfileDetail.StateId).FirstOrDefaultAsync();
            mappedData.CountryName = userCountryDetail.CountryName;
            mappedData.StateName = userStateDetail.StateName;

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = mappedData;
            _response.Messages = "Detail" + ResponseMessages.msgShownSuccess;
            return Ok(_response);
        }
        #endregion

        #region UpdateProfile
        /// <summary>
        ///  Update profile.
        /// </summary>
        [HttpPost]
        [Route("UpdateProfile")]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> UpdateProfile([FromBody] DealerRequestDTO model)
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

            var dealerDetail = await _context.DealerDetail.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (dealerDetail != null)
            {
                dealerDetail.Address1 = model.Address1;
                dealerDetail.Address2 = model.Address2;
            }
          


            _context.Update(dealerDetail);
            await _context.SaveChangesAsync();

            var mappedData = _mapper.Map(model, userDetail);
            _context.Update(userDetail);
            await _context.SaveChangesAsync();

            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(model, userProfileDetail);
            _context.ApplicationUsers.Update(updateProfile);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<DealerResponseDTO>(dealerDetail);
            _mapper.Map(userProfileDetail, response);

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Profile updated successfully.";
            return Ok(_response);
        }
        #endregion

        #region SetDealerStatus
        /// <summary>
        ///  Set dealer status [Pending = 0; Approved = 1; Rejected = 2].
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("SetDealerStatus")]
        public async Task<IActionResult> SetDealerStatus([FromBody] SetDealerStatusDTO model)
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

            var dealer = await _context.DealerDetail.FirstOrDefaultAsync(u => u.DealerId == model.dealerId);

            if (dealer == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            dealer.Status = model.status.ToString();


            _context.Update(dealer);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "dealer status updated successfully.";

            return Ok(_response);
        }

        #endregion

    }
}
