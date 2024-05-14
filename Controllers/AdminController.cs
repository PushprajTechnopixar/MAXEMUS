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
using Twilio.Http;
using TimeZoneConverter;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _contentRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IAccountRepository _userRepo;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IEmailManager _emailSender;
        private ITwilioManager _twilioManager;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment,
            IAdminRepository adminRepository, ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, IContentRepository contentRepository, RoleManager<IdentityRole> roleManager, IEmailManager emailSender, ITwilioManager twilioManager)
        {
            _userRepo = userRepo;
            _response = new();
            _context = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _twilioManager = twilioManager;
            _userManager = userManager;
            _adminRepository = adminRepository;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _contentRepository = contentRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        #region UpdateProfile
        /// <summary>
        ///  Update profile for admin.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateProfile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProfile([FromBody] AdminProfileRequestDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }

            if (Gender.Male.ToString() != model.gender && Gender.Female.ToString() != model.gender && Gender.Other.ToString() != model.gender)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid gender.";
                return Ok(_response);
            }

            if (model.countryId > 0)
            {
                var countryId = await _context.CountryMaster.FindAsync(model.countryId);
                if (countryId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid countryId.";
                    return Ok(_response);
                }
            }
            if (model.stateId > 0)
            {
                var stateId = await _context.StateMaster.FindAsync(model.stateId);
                if (stateId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid stateId.";
                    return Ok(_response);
                }
            }
            var userDetail = await _context.ApplicationUsers.FindAsync(currentUserId);
            if (userDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any user.";
                return Ok(_response);
            }

            var response = await _adminRepository.UpdateProfile(model, currentUserId);
            return Ok(response);
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

            var response = await _adminRepository.GetProfileDetail(currentUserId);
            return Ok(response);

        }
        #endregion

        #region AddOrUpdateBrand
        /// <summary>
        /// Add brand.
        /// </summary>
        [HttpPost]
        [Route("AddOrUpdateBrand")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddOrUpdateBrand(AddBrandDTO model)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var response = await _adminRepository.AddOrUpdateBrand(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetBrandDetail
        /// <summary>
        /// Get brand.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("GetBrandDetail")]
        [Authorize]
        public async Task<IActionResult> GetBrandDetail(int brandId)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var response = await _adminRepository.GetBrandDetail(brandId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetBrandList
        /// <summary>
        ///  Get brand list.
        /// </summary>
        [HttpGet]
        [Route("GetBrandList")]
        [Authorize]
        public async Task<IActionResult> GetBrandList([FromQuery] NullableFilterationListDTO? model, string? CreatedBy)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var response = await _adminRepository.GetBrandList(model, CreatedBy);
                return Ok(response);
            }
            catch (System.Exception ex)
            {

                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region DeleteBrand
        /// <summary>
        /// Delete brand.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("DeleteBrand")]
        [Authorize]
        public async Task<IActionResult> DeleteBrand(int brandId)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var response = await _adminRepository.DeleteBrand(brandId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region AddCategory
        /// <summary>
        ///  Add category.
        /// </summary>
        [HttpPost("AddCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDTO model)
        {
            try
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

                var response = await _adminRepository.AddCategory(model);
                return Ok(response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region UpdateCategory
        /// <summary>
        ///  Update category.
        /// </summary>
        [HttpPost("UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UpdateProductCategory([FromBody] UpdateCategoryDTO model)
        {
            try
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

                var response = await _adminRepository.UpdateProductCategory(model);
                return Ok(response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetCategoryList
        /// <summary>
        ///  Get category list.
        /// </summary>
        [HttpGet("GetCategoryList")]
        [Authorize]
        public async Task<IActionResult> GetCategoryList([FromQuery] GetCategoryRequestDTO model)
        {
            try
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

                var response = await _adminRepository.GetCategoryList(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetCategoryDetail
        /// <summary>
        ///  Get category Detail.
        /// </summary>
        [HttpGet("GetCategoryDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> GetCategoryDetail([FromQuery] GetCategoryDetailRequestDTO model)
        {
            try
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

                var response = await _adminRepository.GetCategoryDetail(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region DeleteCategory
        /// <summary>
        ///  Delete category.
        /// </summary>
        [HttpDelete("DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> DeleteCategory([FromQuery] DeleteCategoryDTO model)
        {
            try
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

                var response = await _adminRepository.DeleteCategory(model);
                return Ok(response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetDistributorList
        /// <summary>
        ///   Get Distributor List.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("GetDistributorList")]
        public async Task<IActionResult> GetDistributorList([FromQuery] FilterationListDTO model)
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

            var distributorUser = await _userManager.GetUsersInRoleAsync(Role.Distributor.ToString());
            if (distributorUser.Count < 1)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Record not found.";
                return Ok(_response);
            }

            var response = await _adminRepository.GetDistributorList(model);

            return Ok(response);
        }
        #endregion

        #region SetDistributorStatus
        /// <summary>
        ///   Set distributor status [Pending = 0; Approved = 1; Rejected = 2]..
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Admin")]
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

            if (model.status != (Status.Pending.ToString())
              && model.status != (Status.Approved.ToString())
              && model.status != (Status.Rejected.ToString()))
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

            var distributorAddress = await _context.DistributorAddress
                .Where(u => u.DistributorId == distributor.DistributorId && u.AddressType == AddressType.Individual.ToString())
                .FirstOrDefaultAsync();

            if (distributorAddress == null && model.status == Status.Approved.ToString())
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Can't update status while profile is incomplete.";
                return Ok(_response);
            }

            var response = await _adminRepository.SetDistributorStatus(model);
            return Ok(response);
        }

        #endregion

        #region GetDealerList
        /// <summary>
        ///   Get Dealer List.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("GetDealerList")]

        public async Task<IActionResult> GetDealerList([FromQuery] DealerFilterationListDTO model)
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

            var dealerUser = await _context.DealerDetail.ToListAsync();

            List<DealerUserListDTO> dealerUserList = new List<DealerUserListDTO>();
            foreach (var item in dealerUser)
            {
                var dealerUserProfileDetail = await _context.ApplicationUsers
                    .Where(u => u.Id == item.UserId && u.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (dealerUserProfileDetail != null)
                {
                    var mappedData = _mapper.Map<DealerUserListDTO>(item);
                    mappedData.dealerId = item.DealerId;
                    mappedData.userId = dealerUserProfileDetail.Id;
                    mappedData.email = dealerUserProfileDetail.Email;
                    mappedData.firstName = dealerUserProfileDetail.FirstName;
                    mappedData.lastName = dealerUserProfileDetail.LastName;
                    mappedData.profilePic = dealerUserProfileDetail.ProfilePic;
                    mappedData.gender = dealerUserProfileDetail.Gender;
                    mappedData.Status = item.Status;
                    mappedData.distributorCode = item.DistributorCode;
                    if (!string.IsNullOrEmpty(item.DistributorCode))
                    {
                        var distributor = await _context.DistributorDetail
                        .Where(u => u.DistributorCode == item.DistributorCode && u.IsDeleted == false)
                        .FirstOrDefaultAsync();
                        mappedData.distributorId = distributor.DistributorId;
                        var Profile = await _context.ApplicationUsers
                        .Where(u => u.Id == distributor.UserId && u.IsDeleted == false)
                        .FirstOrDefaultAsync();
                        if (Profile != null)
                        {
                            mappedData.distributorFirstName = Profile.FirstName;
                            mappedData.distributorLastName = Profile.LastName;
                        }
                    }
                    mappedData.createDate = item.CreateDate.ToShortDateString();

                    if (item.DistributorId != null)
                    {
                        var distributorDetail = await _context.DistributorDetail
                                        .Where(u => u.DistributorId == item.DistributorId && u.IsDeleted == false).FirstOrDefaultAsync();
                        var distributorProfileDetail = await _context.ApplicationUsers
                        .Where(u => u.Id == distributorDetail.UserId).FirstOrDefaultAsync();
                        if (distributorProfileDetail != null)
                        {
                            mappedData.distributorFirstName = dealerUserProfileDetail.FirstName;
                            mappedData.distributorLastName = dealerUserProfileDetail.LastName;
                            mappedData.distributorId = distributorDetail.DistributorId;
                        }
                    }

                    dealerUserList.Add(mappedData);
                }
            }


            dealerUserList = dealerUserList.OrderByDescending(u => u.createDate).ToList();
            if (model.distributorId != null)
            {
                if (model.distributorId > 0)
                {
                    dealerUserList = dealerUserList.Where(u => u.distributorId == model.distributorId).ToList();
                }
            }

            if (!string.IsNullOrEmpty(model.searchQuery))
            {
                dealerUserList = dealerUserList.Where(u => u.firstName.ToLower().Contains(model.searchQuery.ToLower())
                || u.email.ToLower().Contains(model.searchQuery.ToLower())
                ).ToList();
            }

            // Get's No of Rows Count   
            int count = dealerUserList.Count();

            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            int CurrentPage = model.pageNumber;

            // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
            int PageSize = model.pageSize;

            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            // Returns List of Customer after applying Paging   
            var items = dealerUserList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            // if CurrentPage is greater than 1 means it has previousPage  
            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            // if TotalPages is greater than CurrentPage means it has nextPage  
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            // Returing List of Customers Collections  
            FilterationResponseModel<DealerUserListDTO> obj = new FilterationResponseModel<DealerUserListDTO>();
            obj.totalCount = TotalCount;
            obj.pageSize = PageSize;
            obj.currentPage = CurrentPage;
            obj.totalPages = TotalPages;
            obj.previousPage = previousPage;
            obj.nextPage = nextPage;
            obj.searchQuery = string.IsNullOrEmpty(model.searchQuery) ? "no parameter passed" : model.searchQuery;
            obj.dataList = items.ToList();

            if (obj == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Error while adding.";
                return Ok(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = obj;
            _response.Messages = "List shown successfully.";
            return Ok(_response);
        }
        #endregion

        #region OrderList
        /// <summary>
        ///  Get OrderList.
        /// </summary>
        /// <returns></returns>
        [HttpGet("OrderList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> OrderList([FromQuery] OrderFiltrationListDTO model)
        {
            try
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

                List<OrderedListDTO> orderedList = new List<OrderedListDTO>();

                var productList = await _context.Product.ToListAsync();

                var distributorOrders = await _context.DistributorOrder
                         .OrderByDescending(u => u.OrderDate)
                         .Select(u => new OrderedListDTO
                         {
                             OrderId = u.OrderId,
                             UserId = u.UserId,
                             ProductName = _context.DistributorOrderedProduct
                         .Where(x => x.OrderId == u.OrderId)
                         .Select(x => x.Product.Name)
                         .FirstOrDefault(),
                             DistributorId = _context.DistributorDetail
                             .Where(d => d.UserId == u.UserId)
                             .Select(d => d.DistributorId)
                             .FirstOrDefault(),
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             OrderStatus = u.OrderStatus,
                             PaymentMethod = u.PaymentMethod,
                             PaymentStatus = u.PaymentStatus,
                             TotalMrp = u.TotalMrp,
                             TotalDiscountAmount = u.TotalDiscountAmount,
                             TotalSellingPrice = u.TotalSellingPrice,
                             OrderDate = u.OrderDate.ToString(DefaultDateFormat), // Assuming OrderDate is of type DateTime
                             OrderTime = u.OrderDate.ToString("HH:mm"), // Assuming OrderDate is of type DateTime
                             TotalProducts = u.TotalProducts,
                             CancelledBy = u.CancelledBy,
                             PaymentReceipt = u.PaymentReceipt,
                             OrderProductInfo = u.TotalProducts > 1 ? $"{_context.DistributorOrderedProduct.Where(x => x.OrderId == u.OrderId).Select(x => x.Product.Name).FirstOrDefault()} ({u.TotalProducts - 1} More)"
                             : _context.DistributorOrderedProduct.Where(x => x.OrderId == u.OrderId).Select(x => x.Product.Name).FirstOrDefault(),
                             OrderDatTime = u.OrderDate,
                             OrderType = "Distributor"
                         })
                         .ToListAsync();

                var userOrders = await _context.UserOrder
                          //   .Where(u => u.UserId == currentUserId)
                          .OrderByDescending(u => u.OrderDate)
                          .Select(u => new OrderedListDTO
                          {
                              OrderId = u.OrderId,
                              UserId = u.UserId,
                              ProductName = _context.UserOrderedProduct
                                    .Where(x => x.OrderId == u.OrderId)
                                    .Select(x => x.Product.Name)
                                    .FirstOrDefault(),
                              FirstName = u.FirstName,
                              LastName = u.LastName,
                              OrderStatus = u.OrderStatus,
                              PaymentMethod = u.PaymentMethod,
                              PaymentStatus = u.PaymentStatus,
                              TotalMrp = u.TotalMrp,
                              TotalDiscountAmount = u.TotalDiscountAmount,
                              TotalSellingPrice = u.TotalSellingPrice,
                              OrderDate = u.OrderDate.ToString(DefaultDateFormat), // Assuming OrderDate is of type DateTime
                              OrderTime = u.OrderDate.ToString("HH:mm"), // Assuming OrderDate is of type DateTime
                              TotalProducts = u.TotalProducts,
                              CancelledBy = u.CancelledBy,
                              PaymentReceipt = u.PaymentReceipt,
                              OrderProductInfo = u.TotalProducts > 1 ? $"{_context.DistributorOrderedProduct.Where(x => x.OrderId == u.OrderId).Select(x => x.Product.Name).FirstOrDefault()} ({u.TotalProducts - 1} More)"
                              : _context.DistributorOrderedProduct.Where(x => x.OrderId == u.OrderId).Select(x => x.Product.Name).FirstOrDefault(),
                              OrderDatTime = u.OrderDate,
                              OrderType = "User"
                          })
                          .ToListAsync();
                orderedList.AddRange(distributorOrders);
                orderedList.AddRange(userOrders);


                if (model.fromDate != null && model.toDate != null)
                {
                    orderedList = orderedList.Where(x => (x.OrderDatTime.Date >= model.fromDate) && (x.OrderDatTime.Date <= model.toDate)).ToList();
                }

                if (!string.IsNullOrEmpty(model.paymentStatus))
                {
                    orderedList = orderedList.Where(x => (x.PaymentStatus == model.paymentStatus)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(model.orderStatus))
                {
                    orderedList = orderedList.Where(x => (x.OrderStatus == model.orderStatus)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(model.userId) && model.orderType == "User")
                {
                    orderedList = orderedList.Where(x => (x.UserId == model.userId)
                    ).ToList();
                }
                if (!string.IsNullOrEmpty(model.searchQuery))
                {
                    orderedList = orderedList.Where(x => (x.PaymentStatus?.IndexOf(model.searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(model.orderType))
                {
                    orderedList = orderedList.Where(x => (x.OrderType == model.orderType)
                    ).ToList();
                }

                // Get's No of Rows Count   
                int count = orderedList.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = model.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = model.pageSize;

                // Display TotalCount to Records to User  
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging   
                var items = orderedList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Returing List of Customers Collections  
                FilterationResponseModel<OrderedListDTO> obj = new FilterationResponseModel<OrderedListDTO>();
                obj.totalCount = TotalCount;
                obj.pageSize = PageSize;
                obj.currentPage = CurrentPage;
                obj.totalPages = TotalPages;
                obj.previousPage = previousPage;
                obj.nextPage = nextPage;
                obj.searchQuery = string.IsNullOrEmpty(model.searchQuery) ? "no parameter passed" : model.searchQuery;
                obj.dataList = items.ToList();

                if (obj == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgSomethingWentWrong;
                    return Ok(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = obj;
                _response.Messages = "order list shown successfully.";
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region OrderDetail
        /// <summary>
        ///  Get Order Detail.
        /// </summary>
        /// <returns></returns>
        [HttpGet("OrderDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> OrderDetail(long orderId, string? orderType)
        {
            try
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

                DistributorOrderDetailDTO? response = null;

                if (orderType != "User")
                {
                    var distributorOrder = await _context.DistributorOrder.FindAsync(orderId);
                    if (distributorOrder == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    var distributorOrderProducts = await _context.DistributorOrderedProduct
                        .Where(u => u.OrderId == orderId)
                        .ToListAsync();

                    if (distributorOrderProducts == null || distributorOrderProducts.Count == 0)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    response = _mapper.Map<DistributorOrderDetailDTO>(distributorOrder);
                    response.DistributorOrderedProduct = _mapper.Map<List<DistributorOrderedProductDTO>>(distributorOrderProducts);

                    var ctz = TZConvert.GetTimeZoneInfo("India Standard Time");
                    response.OrderDate = (TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(response.OrderDate), ctz)).ToString(DefaultDateFormat);

                    response.DeliveredDate = (TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(distributorOrder.DeliveredDate?.ToString()), ctz)).ToString(DefaultDateFormat);
                }
                else
                {
                    var userOrder = await _context.UserOrder.FindAsync(orderId);
                    if (userOrder == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    var userOrderProducts = await _context.UserOrderedProduct
                        .Where(u => u.OrderId == orderId)
                        .ToListAsync();

                    if (userOrderProducts == null || userOrderProducts.Count == 0)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    response = _mapper.Map<DistributorOrderDetailDTO>(userOrder);
                    response.DistributorOrderedProduct = _mapper.Map<List<DistributorOrderedProductDTO>>(userOrderProducts);

                    var ctz = TZConvert.GetTimeZoneInfo("India Standard Time");
                    response.OrderDate = (TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(response.OrderDate), ctz)).ToString(DefaultDateFormat);

                    response.DeliveredDate = (TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(userOrder.DeliveredDate?.ToString()), ctz)).ToString(DefaultDateFormat);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Order detail shown successfully.";
                _response.Data = response;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region SetOrderStatus
        /// <summary>
        ///  SetOrderStatus.
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetOrderStatus")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> SetOrderStatus(OrderStatusDTO model)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var userDetail = await _userManager.FindByIdAsync(currentUserId);
                if (userDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgUserNotFound;
                    return Ok(_response);
                }

                var roles = await _userManager.GetRolesAsync(userDetail);
                var roleName = roles.FirstOrDefault();

                if (model.orderType != "User")
                {
                    var orderDetail = await _context.DistributorOrder
                        .FirstOrDefaultAsync(u => u.OrderId == model.OrderId);

                    if (orderDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    if (model.type == StatusType.Order.ToString())
                    {
                        orderDetail.OrderStatus = model.status.ToString();
                        orderDetail.CancelledBy = roleName;
                    }
                    if (model.type == StatusType.Payment.ToString())
                    {
                        orderDetail.PaymentStatus = model.status.ToString();
                    }
                    if (model.type == StatusType.PaymentMethod.ToString())
                    {
                        orderDetail.PaymentMethod = model.status.ToString();
                    }

                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var orderDetail = await _context.UserOrder
                        .FirstOrDefaultAsync(u => u.OrderId == model.OrderId);

                    if (orderDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    if (model.type == StatusType.Order.ToString())
                    {
                        orderDetail.OrderStatus = model.status.ToString();
                        orderDetail.CancelledBy = roleName;
                    }
                    if (model.type == StatusType.Payment.ToString())
                    {
                        orderDetail.PaymentStatus = model.status.ToString();
                    }
                    if (model.type == StatusType.PaymentMethod.ToString())
                    {
                        orderDetail.PaymentMethod = model.status.ToString();
                    }

                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Status updated successfully.";
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region AddOrUpdateRewardProduct
        /// <summary>
        /// AddOrUpdateRewardProduct.
        /// </summary>
        [HttpPost]
        [Route("AddOrUpdateRewardProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddOrUpdateRewardProduct(AddOrUpdateRewardProductDTo model)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                RewardProduct? mappedData = null;
                var msg = "";
                if (model.RewardProductId == 0)
                {
                    mappedData = _mapper.Map<RewardProduct>(model);
                    // Add new reward product
                    await _context.RewardProduct.AddAsync(mappedData);
                    msg = "Added successfully.";

                }
                else
                {
                    var rewardPoint = _context.RewardProduct.Where(U => U.RewardProductId == model.RewardProductId).FirstOrDefault();

                    if (rewardPoint == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Not found";
                        return Ok(_response);
                    }
                    // Update existing reward product
                    mappedData = _mapper.Map(model, rewardPoint);

                    _context.Update(rewardPoint);
                    msg = "Updated successfully.";
                }

                await _context.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = msg;
                _response.Data = mappedData;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region getRewadProductList
        /// <summary>
        /// getRewadProductList.
        /// </summary>
        [HttpGet]
        [Route("getRewadProductList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> getRewadProductList()
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var rewards = _context.RewardProduct.Where(u => u.ExpiryDate.Date >= DateTime.Now.Date).ToList();

                var mappedData = _mapper.Map<List<RewardproductListDTO>>(rewards);

                await _context.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "List shown successfully.";
                _response.Data = mappedData;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region getRewadProductDetail
        /// <summary>
        /// getRewadProductDetail.
        /// </summary>
        [HttpGet]
        [Route("getRewadProductDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> getRewadProductList(int RewardProductId)
        {
            try
            {
                string currentUserId = (HttpContext.User.Claims.First().Value);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Token expired.";
                    return Ok(_response);
                }

                var rewards = await _context.RewardProduct.Where(u => u.RewardProductId == RewardProductId).FirstOrDefaultAsync();
                if (rewards == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Not found any record.";
                    return Ok(_response);
                }
                var mappedData = _mapper.Map<RewardproductListDTO>(rewards);

                await _context.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Product detail shown successfully.";
                _response.Data = mappedData;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return Ok(_response);
            }
        }
        #endregion

        #region GetDashboardData
        /// <summary>
        ///  Get Dashboard Data.
        /// </summary>
        [HttpGet]
        [Route("GetDashboardData")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDashboardData()
        {
            var userDetail = _userManager.FindByEmailAsync("admin@maxemus.com").GetAwaiter().GetResult();
            if (userDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            var query = from t1 in _context.Product
                        where !t1.IsDeleted
                        orderby t1.CreateDate descending
                        select new DashboardProductDTO
                        {
                            ProductId = t1.ProductId,
                            MainCategoryId = t1.MainCategoryId,
                            MainCategoryName = (from mc in _context.MainCategory
                                                where mc.MainCategoryId == t1.MainCategoryId
                                                select mc.MainCategoryName).FirstOrDefault(),
                            SubCategoryId = t1.SubCategoryId,
                            SubCategoryName = (from sc in _context.SubCategory
                                               where sc.SubCategoryId == t1.SubCategoryId
                                               select sc.SubCategoryName).FirstOrDefault(),
                            BrandId = t1.BrandId,
                            Model = t1.Model,
                            Name = t1.Name,
                            Description = t1.Description,
                            Image1 = t1.Image1,
                            IsActive = t1.IsActive,
                            TotalMrp = t1.TotalMrp,
                            Discount = t1.Discount,
                            DiscountType = t1.DiscountType,
                            SellingPrice = t1.SellingPrice,
                            CreateDate = t1.CreateDate.ToShortDateString()
                        };

            var products = query.ToList();

            DashboardDTO dashboard = new DashboardDTO();

            DashboardProductViewModel dashboardProduct1 = new DashboardProductViewModel();
            dashboardProduct1.products = query.Take(3).ToList();
            dashboardProduct1.productType = "Latest_Luanches";
            dashboard.latestLaunches = dashboardProduct1;

            List<int?>? popularProductIds = null;

            popularProductIds = _context.OrderedPoduct
                                           .GroupBy(p => p.ProductId)
                                           .Select(g => g.Key)
                                           .ToList();
            if (popularProductIds == null)
            {
                popularProductIds = _context.DistributorOrderedProduct
                                           .GroupBy(p => p.ProductId)
                                           .Select(g => g.Key)
                                           .ToList();
            }
            if (popularProductIds.Count < 6)
            {
                DashboardProductViewModel dashboardProduct2 = new DashboardProductViewModel();
                dashboardProduct2.products = query.ToList().Except(dashboardProduct1.products).Take(6).ToList();
                dashboardProduct2.productType = "Popular_Products";
                dashboard.popular = dashboardProduct2;
            }
            else
            {
                DashboardProductViewModel dashboardProduct2 = new DashboardProductViewModel();
                dashboardProduct2.products = query.ToList().Where(p => popularProductIds.Contains(p.ProductId))
                        .Except(dashboardProduct1.products)
                        .OrderByDescending(x => x.CreateDate)
                        .Take(6)
                        .ToList();
                dashboardProduct2.productType = "Popular_Products";
                dashboard.popular = dashboardProduct2;
            }

            var companyDetail = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == userDetail.Id);

            var adminResponseDTO = _mapper.Map<AdminResponseDTO>(userDetail);

            adminResponseDTO.companyProfile = _mapper.Map<AdminCompanyResponseDTO>(companyDetail);
            dashboard.companyDetail = adminResponseDTO;

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Dashboard data shown successfully.";
            _response.Data = dashboard;
            return Ok(_response);
        }
        #endregion

    }
}
