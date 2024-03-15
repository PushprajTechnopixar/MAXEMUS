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
using System.Net;
using Microsoft.EntityFrameworkCore;
using MaxemusAPI.Common;
using Amazon.Pinpoint;
using System.ComponentModel.Design;
using Twilio.Types;
using static MaxemusAPI.Common.GlobalVariables;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
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

        public AdminController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
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

        #region AddOrUpdateProfile
        /// <summary>
        ///  AddOrUpdateProfile for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateCompanyProfile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddOrUpdateCompanyProfile([FromBody] AdminCompanyDTO model)
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

            if (model.CompanyId == 0)
            {
                var existingCompany = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (existingCompany == null)
                {
                    existingCompany = new CompanyDetail
                    {
                        UserId = currentUserId,
                        CompanyName = model.CompanyName,
                        RegistrationNumber = model.RegistrationNumber,
                        Image = model.Image,
                        CountryId = existingUser.CountryId,
                        StateId = existingUser.StateId,
                        City = model.City,
                        StreetAddress = model.StreetAddress,
                        Landmark = model.Landmark,
                        PostalCode = model.PostalCode,
                        PhoneNumber = model.PhoneNumber,
                        WhatsappNumber = model.WhatsAppNumber,
                        AboutUs = model.AboutUs
                    };

                    _context.Add(existingCompany);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Company added successfully.";
                    return Ok(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Company already exists.";
                return Ok(_response);
            }
            else if (model.CompanyId > 0)
            {
                var existingCompany = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.CompanyId == model.CompanyId && u.UserId == currentUserId);
                if (existingCompany != null)
                {
                    existingCompany.CompanyName = model.CompanyName;
                    existingCompany.RegistrationNumber = model.RegistrationNumber;
                    existingCompany.Image = model.Image;
                    existingCompany.CountryId = existingUser.CountryId;
                    existingCompany.StateId = existingUser.StateId;
                    existingCompany.City = model.City;
                    existingCompany.StreetAddress = model.StreetAddress;
                    existingCompany.Landmark = model.Landmark;
                    existingCompany.PostalCode = model.PostalCode;
                    existingCompany.PhoneNumber = model.PhoneNumber;
                    existingCompany.WhatsappNumber = model.WhatsAppNumber;
                    existingCompany.AboutUs = model.AboutUs;

                    _context.Update(existingCompany);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Company updated successfully.";
                    return Ok(_response);
                }
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = false;
            _response.Messages = "Invalid CompanyId.";
            return Ok(_response);

        }
        #endregion

        #region GetProfileDetail
        /// <summary>
        ///  Get profile.
        /// </summary>
        [HttpGet]
        [Route("GetProfileDetail")]
        [Authorize(Roles = "Admin")]
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
            var companyDetail = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (companyDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            var mappedData = _mapper.Map<AdminResponseDTO>(userDetail);
            _mapper.Map(companyDetail, mappedData);

            var userCountryDetail = await _context.CountryMaster.Where(u => u.CountryId == mappedData.CountryId).FirstOrDefaultAsync();
            var userStateDetail = await _context.StateMaster.Where(u => u.StateId == mappedData.StateId).FirstOrDefaultAsync();
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserRequestDTO model)
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
