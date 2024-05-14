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
        public async Task<IActionResult> GetProfileDetail(int? dealerId)
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
            DealerDetail? dealerDetail = null;
            dealerDetail = await _context.DealerDetail.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (dealerDetail == null)
            {
                dealerDetail = await _context.DealerDetail.Where(u => u.DealerId == dealerId).FirstOrDefaultAsync();
            }
            var mappedData = _mapper.Map<DealerResponseDTO>(userDetail);
            _mapper.Map(dealerDetail, mappedData);
            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            _mapper.Map(userProfileDetail, mappedData);

            var userCountryDetail = await _context.CountryMaster.Where(u => u.CountryId == userProfileDetail.CountryId).FirstOrDefaultAsync();
            var userStateDetail = await _context.StateMaster.Where(u => u.StateId == userProfileDetail.StateId).FirstOrDefaultAsync();
            if (userCountryDetail != null)
            {
                mappedData.CountryName = userCountryDetail.CountryName;
            }
            if (userStateDetail != null)
            {
                mappedData.StateName = userStateDetail.StateName;
            }

            if (dealerDetail != null)
            {
                if (!string.IsNullOrEmpty(dealerDetail.DistributorCode))
                {
                    var distributorDetail = await _context.DistributorDetail
                        .Where(u => u.DistributorCode == dealerDetail.DistributorCode && u.IsDeleted == false)
                        .FirstOrDefaultAsync();
                    var distributorUserProfileDetail = await _context.ApplicationUsers
                        .Where(u => u.Id == distributorDetail.UserId && u.IsDeleted == false)
                        .FirstOrDefaultAsync();

                    var distributorAddress = await _context.DistributorAddress
                    .Where(u => u.DistributorId == distributorDetail.DistributorId && u.AddressType == AddressType.Individual.ToString())
                    .FirstOrDefaultAsync();

                    if (distributorUserProfileDetail != null && distributorAddress != null)
                    {
                        var distributor = _mapper.Map<DistributorDetailForDealerDTO>(distributorDetail);
                        distributor.companyName = distributorDetail.Name;
                        distributor.email = distributorAddress.Email;
                        distributor.firstName = distributorUserProfileDetail.FirstName;
                        distributor.lastName = distributorUserProfileDetail.LastName;
                        distributor.phoneNumber = distributorAddress.PhoneNumber;
                        distributor.streetAddress = distributorAddress?.StreetAddress;
                        mappedData.distributor = distributor;
                        distributor.BuildingNameOrNumber = distributorAddress.BuildingNameOrNumber;
                    }
                }
            }

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

            var distributorDetail = await _context.DistributorDetail.Where(u => u.DistributorCode == model.distributorCode && u.IsDeleted == false).FirstOrDefaultAsync();
            if (distributorDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            if (model.countryId > 0)
            {
                var countryId = await _context.CountryMaster.FindAsync(model.countryId);
            }
            if (model.stateId > 0)
            {
                var stateId = await _context.StateMaster.FindAsync(model.stateId);
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
            if (Gender.Male.ToString() != model.gender && Gender.Female.ToString() != model.gender && Gender.Other.ToString() != model.gender)
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
                    CreateDate = DateTime.UtcNow,
                    DistributorCode = model.distributorCode,
                    Status = Status.Pending.ToString(),
                };

                _context.Add(dealerDetail);
            }
            else
            {
                dealerDetail.Address1 = model.Address1;
                dealerDetail.Address2 = model.Address2;
                dealerDetail.ModifyDate = DateTime.UtcNow;
                dealerDetail.DistributorCode = model.distributorCode;
                if (!string.IsNullOrEmpty(dealerDetail.DistributorCode))
                {
                    if (dealerDetail.DistributorCode != model.distributorCode)
                    {
                        dealerDetail.Status = Status.Pending.ToString();
                    }
                }
                else
                {
                    dealerDetail.Status = Status.Pending.ToString();
                }

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

            if (userCountry != null && userState != null)
            {
                response.CountryName = userCountry.CountryName;
                response.StateName = userState.StateName;
            }

            var distributorUserProfileDetail = await _context.ApplicationUsers
                .Where(u => u.Id == distributorDetail.UserId && u.IsDeleted == false)
                .FirstOrDefaultAsync();

            var distributorAddress = await _context.DistributorAddress
            .Where(u => u.DistributorId == distributorDetail.DistributorId && u.AddressType == AddressType.Individual.ToString())
            .FirstOrDefaultAsync();

            if (distributorUserProfileDetail != null && distributorAddress != null)
            {
                var distributor = _mapper.Map<DistributorDetailForDealerDTO>(distributorDetail);

                distributor.companyName = distributorDetail.Name;
                distributor.email = distributorAddress.Email;
                distributor.firstName = distributorUserProfileDetail.FirstName;
                distributor.lastName = distributorUserProfileDetail.LastName;
                distributor.phoneNumber = distributorAddress.PhoneNumber;
                distributor.streetAddress = distributorAddress?.StreetAddress;
                response.distributor = distributor;
                distributor.BuildingNameOrNumber = distributorAddress.BuildingNameOrNumber;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Profile updated successfully.";
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
        public async Task<IActionResult> ScanProduct([FromBody] ScanProductDTO model)
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

            var product = _context.ProductStock.FirstOrDefault(p => p.SerialNumber == model.serialNumber);
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

            if (dealerDetail.Status != Status.Approved.ToString())
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Dealer is not approved."
                });
            }

            var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(p => p.DistributorCode == dealerDetail.DistributorCode && p.IsDeleted == false);
            if (distributorDetail == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Distributor details not found."
                });
            }

            var dealerProduct = await _context.DealerProduct.FirstOrDefaultAsync(p => p.ProductStockId == product.ProductStockId);
            if (dealerProduct != null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "This product has been already scanned."
                });
            }

            int rewardPoint = 0;
            if (!string.IsNullOrEmpty(model.serialNumber) && model.serialNumber.Length >= 2)
            {
                char firstChar = char.ToUpper(model.serialNumber[0]);
                char secondChar = char.ToUpper(model.serialNumber[1]);

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
                    int point = (secondChar - 'A');
                    rewardPoint = point == 0 ? rewardPoint * 10 : rewardPoint * 10 * point;
                }
                else
                {
                    rewardPoint = 0;
                }
            }

            var dealer = new DealerProduct
            {
                DealerId = dealerDetail.DealerId,
                DistributorId = distributorDetail.DistributorId,
                ProductId = product.ProductId,
                ProductStockId = product.ProductStockId,
                RewardPoint = rewardPoint,
                Status = "Scanned",
            };

            _context.Add(dealer);
            await _context.SaveChangesAsync();

            // var dealerSellingPoint = new PointDetail
            // {
            //     Points = (double)rewardPoint,
            //     RedeemedPoints = 0,
            //     UserId = dealerDetail.UserId,
            //     Status = PointStatus.Active.ToString(),
            //     DealerProductId = dealer.DealerProductId,
            // };

            // _context.Add(dealerSellingPoint);
            // await _context.SaveChangesAsync();

            var pointDetail = await _context.Points.Where(x => x.UserId == dealerDetail.UserId).FirstOrDefaultAsync();
            if (pointDetail != null)
            {
                pointDetail.ActivePoints = pointDetail.ActivePoints + (double)rewardPoint;
                _context.Update(pointDetail);
                await _context.SaveChangesAsync();
            }
            else
            {
                var points = new Points
                {
                    ActivePoints = (double)rewardPoint,
                    UserId = dealerDetail.UserId,
                };
                _context.Add(points);
                await _context.SaveChangesAsync();
            }

            // var distributorSellingPoint = new PointDetail
            // {
            //     Points = ((double)rewardPoint / 2),
            //     RedeemedPoints = 0,
            //     UserId = distributorDetail.UserId,
            //     Status = PointStatus.Active.ToString(),
            //     DealerProductId = dealer.DealerProductId,
            // };

            // _context.Add(distributorSellingPoint);
            // await _context.SaveChangesAsync();

            pointDetail = await _context.Points.Where(x => x.UserId == distributorDetail.UserId).FirstOrDefaultAsync();
            if (pointDetail != null)
            {
                pointDetail.ActivePoints = pointDetail.ActivePoints + (double)rewardPoint;
                _context.Update(pointDetail);
                await _context.SaveChangesAsync();
            }
            else
            {
                var points = new Points
                {
                    ActivePoints = ((double)rewardPoint / 2),
                    UserId = distributorDetail.UserId,
                };
                _context.Add(points);
                await _context.SaveChangesAsync();
            }

            product.Status = SerialNumberStatus.Scanned.ToString();
            _context.Update(product);
            await _context.SaveChangesAsync();
            var response = _mapper.Map<DealerProductDTO>(dealer);
            response.CreateDate = dealer.CreateDate.ToShortDateString();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Product added successfully.";
            _response.Data = response;

            return Ok(_response);
        }
        #endregion

        #region VerifyDistributor
        /// <summary>
        ///   VerifyDistributor.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [Route("VerifyDistributor")]
        public async Task<IActionResult> VerifyDistributor(string distributorCode)
        {
            try
            {
                var distributorUser = await _context.DistributorDetail.Where(u => u.DistributorCode == distributorCode && u.Status == Status.Approved.ToString()).FirstOrDefaultAsync();
                if (distributorUser == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgUserNotFound;
                    return Ok(_response);
                }
                var distributorUserProfileDetail = await _context.ApplicationUsers
                    .Where(u => u.Id == distributorUser.UserId && u.IsDeleted == false)
                    .FirstOrDefaultAsync();

                var distributorAddress = await _context.DistributorAddress
                .Where(u => u.DistributorId == distributorUser.DistributorId && u.AddressType == AddressType.Individual.ToString())
                .FirstOrDefaultAsync();

                DistributorDetailForDealerDTO? mappedData = null;

                if (distributorUserProfileDetail != null && distributorAddress != null)
                {
                    mappedData = _mapper.Map<DistributorDetailForDealerDTO>(distributorUser);

                    mappedData.companyName = distributorUser.Name;
                    mappedData.email = distributorAddress.Email;
                    mappedData.firstName = distributorUserProfileDetail.FirstName;
                    mappedData.lastName = distributorUserProfileDetail.LastName;
                    mappedData.phoneNumber = distributorAddress.PhoneNumber;
                    mappedData.streetAddress = distributorAddress?.StreetAddress;
                    mappedData.BuildingNameOrNumber = distributorAddress.BuildingNameOrNumber;
                }

                if (mappedData != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Verified successfully.";
                    _response.Data = mappedData;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Wrong distributor code.";
                    return Ok(_response);
                }
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

        #region getScannedProductList
        /// <summary>
        ///  Get getScannedProductList.
        /// </summary>
        /// <returns></returns>
        [HttpGet("getScannedProductList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> getScannedProductList([FromQuery] ScannedFiltrationListDTO model)
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
                DealerDetail? dealerDetail = null;
                var roles = await _userManager.GetRolesAsync(userDetail);
                var roleName = roles.FirstOrDefault();
                if (roleName != "Dealer")
                {
                    if (!string.IsNullOrEmpty(model.userId))
                    {
                        dealerDetail = await _context.DealerDetail.Where(dp => dp.UserId == model.userId).FirstOrDefaultAsync();
                        if (dealerDetail == null)
                        {
                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = false;
                            _response.Messages = "Not found any record.";
                            return Ok(_response);
                        }
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Please enter dealer id.";
                        return Ok(_response);
                    }
                }
                else
                {
                    dealerDetail = await _context.DealerDetail.Where(dp => dp.UserId == currentUserId).FirstOrDefaultAsync();
                }
                if (dealerDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgUserNotFound;
                    return Ok(_response);
                }

                List<ScannedProductlistDTO> productList;

                var dealerProducts = await _context.DealerProduct
                .Include(dp => dp.Product).Include(dp => dp.ProductStock)
                .Where(u => u.DealerId == dealerDetail.DealerId)
                .Select(dp => new ScannedProductlistDTO
                {
                    DealerProductId = dp.DealerProductId,
                    DealerId = dp.DealerId,
                    DistributorId = dp.DistributorId,
                    ProductId = dp.ProductId,
                    RewardPoint = dp.RewardPoint,
                    MainCategoryId = dp.Product.MainCategoryId,
                    SubCategoryId = dp.Product.SubCategoryId,
                    BrandId = dp.Product.BrandId,
                    Model = dp.Product.Model,
                    Name = dp.Product.Name,
                    Description = dp.Product.Description,
                    Image1 = dp.Product.Image1,
                    TotalMrp = dp.Product.TotalMrp,
                    Discount = dp.Product.Discount,
                    SellingPrice = dp.Product.SellingPrice,
                    Status = dp.Status,
                    SerialNumber = dp.ProductStock.SerialNumber,
                    ScannedDate = dp.CreateDate.ToString(DefaultDateFormat),
                    // Add other properties from Product table as needed
                })
                .ToListAsync();

                if (!string.IsNullOrEmpty(model.searchQuery))
                {
                    dealerProducts = dealerProducts.Where(x => (x.Name?.IndexOf(model.searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }

                // Get's No of Rows Count   
                int count = dealerProducts.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = model.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = model.pageSize;

                // Display TotalCount to Records to User  
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging   
                var items = dealerProducts.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Returing List of Customers Collections  
                FilterationResponseModel<ScannedProductlistDTO> obj = new FilterationResponseModel<ScannedProductlistDTO>();
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
                _response.Messages = "Product list shown successfully.";
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

        #region ReturnProduct
        /// <summary>
        ///  ReturnProduct.
        /// </summary>
        [HttpPost("ReturnProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Dealer")]
        public async Task<IActionResult> ReturnProduct([FromBody] ScanProductDTO model)
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

            var product = _context.ProductStock.FirstOrDefault(p => p.SerialNumber == model.serialNumber);
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

            if (dealerDetail.Status != Status.Approved.ToString())
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Dealer is not approved."
                });
            }

            var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(p => p.DistributorCode == dealerDetail.DistributorCode && p.IsDeleted == false);
            if (dealerDetail == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Dealer details not found."
                });
            }

            var dealerProduct = await _context.DealerProduct.FirstOrDefaultAsync(p => p.ProductStockId == product.ProductStockId);
            if (dealerProduct == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Product has not scanned yet."
                });
            }

            dealerProduct.Status = "Returned";

            _context.Add(dealerProduct);
            await _context.SaveChangesAsync();

            var dealerSellingPoint = _context.PointDetail.Where(u => u.DealerProductId == dealerProduct.DealerProductId && u.UserId == dealerDetail.UserId).FirstOrDefault();

            dealerSellingPoint.Status = PointStatus.Cancelled.ToString();

            _context.Add(dealerSellingPoint);
            await _context.SaveChangesAsync();

            var distributorSellingPoint = _context.PointDetail.Where(u => u.DealerProductId == dealerProduct.DealerProductId && u.UserId == distributorDetail.UserId).FirstOrDefault();

            distributorSellingPoint.Status = PointStatus.Cancelled.ToString();

            _context.Add(distributorSellingPoint);
            await _context.SaveChangesAsync();

            product.Status = SerialNumberStatus.Active.ToString();
            _context.Add(distributorSellingPoint);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Product returned successfully.";

            return Ok(_response);
        }
        #endregion

        #region getRewardPoint
        /// <summary>
        ///  getRewardPoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("getRewardPoint")]
        public async Task<IActionResult> getRewardPoint(int? dealerId)
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
            DealerDetail? dealerDetail;
            var roles = await _userManager.GetRolesAsync(currentUserDetail);
            var roleName = roles.FirstOrDefault();
            if (roleName != "Dealer")
            {
                if (dealerId != null)
                {
                    dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Dealer id is required.";
                    return Ok(_response);
                }
            }
            else
            {
                dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            }

            if (dealerDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            // var poitDetail = _context.PointDetail.Where(u => u.UserId == dealerDetail.UserId).ToList();
            // var activePoints = poitDetail.Where(u => u.Status == PointStatus.Active.ToString()).Sum(u => u.Points);
            // var redeemedPoints = poitDetail.Where(u => u.Status == PointStatus.Redeemed.ToString()).Sum(u => u.RedeemedPoints);

            var poitDetail = _context.Points.Where(u => u.UserId == dealerDetail.UserId).FirstOrDefault();

            double redeemedPoints = 0;
            double activePoints = 0;

            if (poitDetail != null)
            {
                activePoints = (double)poitDetail.ActivePoints;
                redeemedPoints = (double)poitDetail.RedeemedPoints;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Point detail shown successfully.";
            _response.Data = new { activePoints = activePoints, redeemedPoints = redeemedPoints };

            return Ok(_response);
        }
        #endregion

        #region GetProductDetail
        /// <summary>
        ///  Get product detail.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("GetProductDetail")]
        public async Task<IActionResult> GetProductDetail(int productId)
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

            var roles = await _userManager.GetRolesAsync(currentUserDetail);

            var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == productId && u.IsDeleted != true);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            var cameraVariants = await _context.CameraVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var audioVariants = await _context.AudioVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var certificationVariants = await _context.CertificationVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var environmentVariants = await _context.EnvironmentVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var generalVariants = await _context.GeneralVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var lensVariants = await _context.LensVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var networkVariants = await _context.NetworkVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var powerVariants = await _context.PowerVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var videoVariants = await _context.VideoVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            // var accessoriesVariants = await _context.AccessoriesVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var userMannual = await _context.UserManual.Where(u => u.ProductId == productId).ToListAsync();

            var installationDocumentVariants = await _context.InstallationDocumentVariants.Where(u => u.ProductId == productId).ToListAsync();

            var response = _mapper.Map<DealerProductResponsesDTO>(product);
            response.CreateDate = product.CreateDate.ToString("dd-MM-yyyy");

            var productImageList = new List<ProductImageDTO>();
            if (!string.IsNullOrEmpty(product.Image1))
            {
                var productImage = new ProductImageDTO();
                productImage.productImage = product.Image1;
                productImageList.Add(productImage);
            }
            if (!string.IsNullOrEmpty(product.Image2))
            {
                var productImage = new ProductImageDTO();
                productImage.productImage = product.Image2;
                productImageList.Add(productImage);
            }
            if (!string.IsNullOrEmpty(product.Image3))
            {
                var productImage = new ProductImageDTO();
                productImage.productImage = product.Image3;
                productImageList.Add(productImage);
            }
            if (!string.IsNullOrEmpty(product.Image4))
            {
                var productImage = new ProductImageDTO();
                productImage.productImage = product.Image4;
                productImageList.Add(productImage);
            }
            if (!string.IsNullOrEmpty(product.Image5))
            {
                var productImage = new ProductImageDTO();
                productImage.productImage = product.Image5;
                productImageList.Add(productImage);
            }
            var rewardPoint = _context.ProductStock.Where(u => u.ProductId == product.ProductId).FirstOrDefault();
            response.RewardPoint = rewardPoint != null ? rewardPoint.RewardPoint : 0;
            response.ProductImage = productImageList;
            // response.Accessories = _mapper.Map<AccessoriesVariantsDTO>(accessoriesVariants);

            List<VariantModel> listAudioVariants = new List<VariantModel>();
            AudioVariantsDTO audioVariantsDTO = new AudioVariantsDTO();
            _mapper.Map(audioVariants, audioVariantsDTO);

            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(AudioVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(audioVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listAudioVariants.Add(variantCamera);
                }
            }

            response.Audio = listAudioVariants;

            List<VariantModel> listCameraVariants = new List<VariantModel>();
            CameraVariantsDTO cameraVariantsDTO = new CameraVariantsDTO();
            _mapper.Map(cameraVariants, cameraVariantsDTO);

            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(CameraVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(cameraVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listCameraVariants.Add(variantCamera);
                }
            }

            response.Camera = listCameraVariants;

            List<VariantModel> listCertificationVariants = new List<VariantModel>();
            CertificationVariantsDTO certificationVariantsDTO = new CertificationVariantsDTO();
            _mapper.Map(certificationVariants, certificationVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(CertificationVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(certificationVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listCertificationVariants.Add(variantCamera);
                }
            }
            response.Certification = listCertificationVariants;

            List<VariantModel> listEnvironmentVariants = new List<VariantModel>();
            // Iterate through the properties of CameraVariantsDTO
            EnvironmentVariantsDTO environmentVariantsDTO = new EnvironmentVariantsDTO();
            _mapper.Map(environmentVariants, environmentVariantsDTO);
            foreach (var property in typeof(EnvironmentVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(environmentVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listEnvironmentVariants.Add(variantCamera);
                }
            }
            response.Environment = listEnvironmentVariants;

            List<VariantModel> listGeneralVariants = new List<VariantModel>();
            GeneralVariantsDTO generalVariantsDTO = new GeneralVariantsDTO();
            _mapper.Map(generalVariants, generalVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(GeneralVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(generalVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listGeneralVariants.Add(variantCamera);
                }
            }
            response.General = listGeneralVariants;

            List<VariantModel> listLensVariants = new List<VariantModel>();
            LensVariantsDTO lensVariantsDTO = new LensVariantsDTO();
            _mapper.Map(lensVariants, lensVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(LensVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(lensVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listLensVariants.Add(variantCamera);
                }
            }
            response.Lens = listLensVariants;

            List<VariantModel> listNetworkVariants = new List<VariantModel>();
            NetworkVariantsDTO networkVariantsDTO = new NetworkVariantsDTO();
            _mapper.Map(networkVariants, networkVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(NetworkVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(networkVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listNetworkVariants.Add(variantCamera);
                }
            }
            response.Network = listNetworkVariants;

            List<VariantModel> listPowerVariants = new List<VariantModel>();
            PowerVariantsDTO powerVariantsDTO = new PowerVariantsDTO();
            _mapper.Map(powerVariants, powerVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(PowerVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(powerVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listPowerVariants.Add(variantCamera);
                }
            }
            response.Power = listPowerVariants;

            List<VariantModel> listVideoVariants = new List<VariantModel>();
            VideoVariantsDTO videoVariantsDTO = new VideoVariantsDTO();
            _mapper.Map(videoVariants, videoVariantsDTO);
            // Iterate through the properties of CameraVariantsDTO
            foreach (var property in typeof(VideoVariantsDTO).GetProperties())
            {
                VariantModel variantCamera = new VariantModel();
                variantCamera.data = property.Name;
                variantCamera.name = property.GetValue(videoVariantsDTO)?.ToString(); // Get the value of the property
                if (variantCamera.name != null)
                {
                    listVideoVariants.Add(variantCamera);
                }
            }
            response.Video = listVideoVariants;

            response.installationDocument = _mapper.Map<List<InstallationDocumentDTO>>(installationDocumentVariants);
            response.userMannual = _mapper.Map<List<UserMannualDTO>>(userMannual);

            if (roles.FirstOrDefault() == "Distributor")
            {
                response.TotalMrp = 0;
                response.Discount = response.DistributorDiscount;
                response.DiscountType = response.DistributorDiscountType;
                response.SellingPrice = response.DistributorSellingPrice;
                response.DistributorDiscount = 0;
                response.DistributorDiscountType = 0;
                response.DistributorSellingPrice = 0;
            }
            if (roles.FirstOrDefault() == "Dealer")
            {
                response.DistributorDiscount = 0;
                response.DistributorDiscountType = 0;
                response.DistributorSellingPrice = 0;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Product Detail shown successfully.";

            return Ok(_response);
        }
        #endregion




    }
}
