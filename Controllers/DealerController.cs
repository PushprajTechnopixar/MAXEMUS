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

        #region AddOrUpdateProfile
        /// <summary>
        ///  AddOrUpdateProfile for Dealer.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateProfile")]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> AddOrUpdateProfile([FromBody] DealerProfileDTO model)
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

            if (model.DealerId == 0)
            {
                var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (dealerDetail != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "profile is already present.";
                    return Ok(_response);
                }

                dealerDetail.UserId = currentUserId;
                dealerDetail.Address1 = model.Address1;
                dealerDetail.Address2 = model.Address2;

                _context.Add(dealerDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile added successfully.";
                return Ok(_response);

            }

            if (model.DealerId > 0)
            {
                var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.DealerId == model.DealerId && u.UserId == currentUserId);
                if (dealerDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "not found any record.";
                    return Ok(_response);
                }

                dealerDetail.Address1 = model.Address1;
                dealerDetail.Address2 = model.Address2;

                _context.Update(dealerDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                return Ok(_response);

            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "profile added successfully.";
            return Ok(_response);
        }
        #endregion

    

    }
}
