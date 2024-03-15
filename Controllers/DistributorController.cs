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

        #region AddAdderess
        /// <summary>
        ///  AddAddress for Dealer.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddAddress")]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddAddress([FromBody] DistributorAddressDTO model)
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



            DistributorAddress distributorAddress = new DistributorAddress();
        

            _mapper.Map(userProfileDetail, distributorAddress);
            _mapper.Map(model, distributorAddress);

            _context.Add(distributorAddress);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<DistributorAddressResponseDTO>(distributorAddress);

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Address added successfully.";
            return Ok(_response);

        }
        #endregion





        //#region AddOrUpdateProfile
        ///// <summary>
        /////  AddOrUpdateProfile for Dealer.
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("AddOrUpdateProfile")]
        //[Authorize(Roles = "Distributor")]
        //public async Task<IActionResult> AddOrUpdateProfile([FromBody] DistributorDetailDTO model)
        //{
        //    string currentUserId = (HttpContext.User.Claims.First().Value);
        //    if (string.IsNullOrEmpty(currentUserId))
        //    {
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = false;
        //        _response.Messages = "Token expired.";
        //        return Ok(_response);
        //    }

        //    var existingUser = await _context.ApplicationUsers.FindAsync(currentUserId);
        //    if (existingUser == null)
        //    {
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = false;
        //        _response.Messages = "not found any user.";
        //        return Ok(_response);
        //    }

        //    if (model.DistributorId == 0)
        //    {
        //        var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
        //        if (distributorDetail != null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = false;
        //            _response.Messages = "profile is already present.";
        //            return Ok(_response);
        //        }

        //        distributorDetail.UserId = currentUserId;
        //distributorDetail.AddressId = await _context.DistributorAddress.Where(u => u.DistributorId)
        //distributorDetail.Name
        //distributorDetail.RegistrationNumber
        //distributorDetail.Description
        //distributorDetail.Image
        //distributorDetail.Status
        //distributorDetail.IsDeleted
        //distributorDetail.CreateDate
        //distributorDetail.ModifyDate

        //_context.Add(distributorDetail);
        //        await _context.SaveChangesAsync();

        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = true;
        //        _response.Messages = "Profile added successfully.";
        //        return Ok(_response);

        //    }

        //    if (model.DealerId > 0)
        //    {
        //        var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.DealerId == model.DealerId && u.UserId == currentUserId);
        //        if (dealerDetail == null)
        //        {
        //            _response.StatusCode = HttpStatusCode.OK;
        //            _response.IsSuccess = false;
        //            _response.Messages = "not found any record.";
        //            return Ok(_response);
        //        }

        //        dealerDetail.Address1 = model.Address1;
        //        dealerDetail.Address2 = model.Address2;

        //        _context.Update(dealerDetail);
        //        await _context.SaveChangesAsync();

        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = true;
        //        _response.Messages = "Profile updated successfully.";
        //        return Ok(_response);

        //    }

        //    _response.StatusCode = HttpStatusCode.OK;
        //    _response.IsSuccess = true;
        //    _response.Messages = "profile added successfully.";
        //    return Ok(_response);
        //}
        //#endregion
    }
}
