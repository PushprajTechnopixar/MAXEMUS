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
using MimeKit;
using GSF;
using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        #region GetProfileDetail
        /// <summary>
        ///  Get profile.
        /// </summary>
        [HttpGet("GetProfileDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            _mapper.Map(userProfileDetail, mappedData);

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
            var countryId = await _context.CountryMaster.FindAsync(model.countryId);
            if (countryId == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "please enter valid countryId.";
                return Ok(_response);
            }
            var stateId = await _context.StateMaster.FindAsync(model.stateId);
            if (stateId == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "please enter valid stateId.";
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

            if (dealerDetail == null)
            {
                dealerDetail = new DealerDetail
                {
                    UserId = currentUserId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    CreateDate = DateTime.UtcNow
                };

                _context.Add(dealerDetail);
            }
            else
            {
                dealerDetail.Address1 = model.Address1;
                dealerDetail.Address2 = model.Address2;
                dealerDetail.ModifyDate = DateTime.UtcNow;

                _context.Update(dealerDetail);
            }

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

            var userCountry = await _context.CountryMaster.Where(u => u.CountryId == response.countryId).FirstOrDefaultAsync();
            var userState = await _context.StateMaster.Where(u => u.StateId == response.stateId).FirstOrDefaultAsync();

            response.CountryName = userCountry.CountryName;
            response.StateName = userState.StateName;

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Profile updated successfully.";
            return Ok(_response);
        }
        #endregion

        #region GetProductList
        /// <summary>
        /// Get Product List.
        /// </summary>
        [HttpGet("GetProductList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> GetProductList([FromQuery] ProductFiltrationListDTO model)
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

            var query = from t1 in _context.DistributorOrderedProduct
                        join t2 in _context.Product on t1.ProductId equals t2.ProductId
                        orderby t1.CreateDate descending
                        select new ProductResponselistDTO
                        {
                            ProductId = t2.ProductId,
                            MainCategoryId = t2.MainCategoryId,
                            MainCategoryName = _context.MainCategory.FirstOrDefault(u => u.MainCategoryId == t2.MainCategoryId).MainCategoryName,
                            SubCategoryId = t2.SubCategoryId,
                            SubCategoryName = _context.SubCategory.FirstOrDefault(u => u.SubCategoryId == t2.SubCategoryId).SubCategoryName,
                            BrandId = t2.BrandId,
                            Model = t2.Model,
                            Name = t2.Name,
                            Description = t2.Description,
                            Image1 = t2.Image1,
                            Image2 = t2.Image2,
                            Image3 = t2.Image3,
                            Image4 = t2.Image4,
                            Image5 = t2.Image5,
                            IsActive = t2.IsActive,
                            TotalMrp = t2.TotalMrp,
                            Discount = t2.Discount,
                            DiscountType = t2.DiscountType,
                            SellingPrice = t2.SellingPrice,
                            RewardPoint = t2.RewardPoint,
                            CreateDate = t2.CreateDate.ToShortDateString(),
                        };

            var productList = query.ToList();

            if (!string.IsNullOrEmpty(model.mainCategoryName))
            {
                productList = productList
                    .Where(u => u.MainCategoryName.ToLower().Contains(model.mainCategoryName.ToLower()))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(model.subCategoryName))
            {
                productList = productList
                    .Where(u => u.SubCategoryName.ToLower().Contains(model.subCategoryName.ToLower()))
                    .ToList();
            }
            if (model.mainProductCategoryId > 0)
            {
                productList = productList.Where(u => u.MainCategoryId == model.mainProductCategoryId).ToList();
            }
            if (model.subProductCategoryId > 0)
            {
                productList = productList.Where(u => u.SubCategoryId == model.subProductCategoryId).ToList();
            }
            if (model.brandId > 0)
            {
                productList = productList.Where(u => u.BrandId == model.brandId).ToList();
            }

            if (!string.IsNullOrEmpty(model.searchQuery))
            {
                model.searchQuery = model.searchQuery.TrimEnd();
                productList = productList
                    .Where(u => u.Name.ToLower().Contains(model.searchQuery.ToLower())
                                 || u.Model.ToLower().Contains(model.searchQuery.ToLower()))
                    .ToList();
            }

            int count = productList.Count();
            int CurrentPage = model.pageNumber;
            int PageSize = model.pageSize;
            int TotalCount = count;
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            var items = productList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            var previousPage = CurrentPage > 1 ? "Yes" : "No";
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            FilterationResponseModel<ProductResponselistDTO> obj = new FilterationResponseModel<ProductResponselistDTO>();

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
            _response.Messages = "Item" + ResponseMessages.msgListFoundSuccess;
            return Ok(_response);
        }
        #endregion

        #region ScanProduct
        /// <summary>
        ///  Scan Product.
        /// </summary>
        [HttpPost("ScanProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> ScanProduct(string serialNumber)
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

            var product = _context.ProductStock.FirstOrDefault(p => p.SerialNumber == serialNumber);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Record not found.";
                return Ok(_response);
            }

            var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(p => p.UserId == currentUserId);
            if (dealerDetail == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Dealer details not found."
                });
            }

            var distributorOrderedProduct = await _context.DistributorOrderedProduct.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            var distributorOrder = await _context.DistributorOrder.FirstOrDefaultAsync(p => p.OrderId == distributorOrderedProduct.OrderId);
            var distributorId = await _context.DistributorDetail.FirstOrDefaultAsync(p => p.UserId == distributorOrder.UserId);

            var dealerProduct = await _context.DealerProduct.FirstOrDefaultAsync(p => p.ProductStockId == product.ProductStockId);
            if (dealerProduct != null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Dealer product already exists."
                });
            }

            int rewardPoint = 0;
            if (!string.IsNullOrEmpty(serialNumber) && serialNumber.Length >= 2)
            {
                char firstChar = char.ToUpper(serialNumber[0]);
                char secondChar = char.ToUpper(serialNumber[1]);

                if (firstChar >= 'A' && firstChar <= 'J')
                {
                    rewardPoint += (firstChar - 'A');
                }
                else
                {
                    rewardPoint = 0;
                }

                if (secondChar >= 'A' && secondChar <= 'J')
                {
                    rewardPoint += (secondChar - 'A');
                }
                else
                {
                    rewardPoint = 0;
                }
            }

            var dealer = new DealerProduct
            {
                DealerId = dealerDetail.DealerId,
                DistributorId = distributorId.DistributorId,
                ProductId = product.ProductId,
                ProductStockId = product.ProductStockId,
                RewardPoint = rewardPoint,
            };

            _context.Add(dealer);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<DealerProductDTO>(dealer);
            response.CreateDate = dealer.CreateDate.ToShortDateString();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "dealer product added successfully.";
            _response.Data = response;

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
