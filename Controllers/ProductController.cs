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
using Microsoft.AspNetCore.Authorization;
using MaxemusAPI.Models.Dtos;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using Twilio.Http;
using MaxemusAPI.Common;
using static Google.Apis.Requests.BatchRequest;
using AutoMapper.Configuration.Annotations;
using static MaxemusAPI.Common.GlobalVariables;
using Microsoft.IdentityModel.Tokens;
using MaxemusAPI.Repository;
using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing;
using ZXing;
using ZXing.Common;
using Twilio.Rest.Trusthub.V1.TrustProducts;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IContentRepository _contentRepository;
        private readonly IAccountRepository _userRepo;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;
        private readonly IUploadRepository _uploadRepository;
        private readonly IEmailManager _emailSender;
        private ITwilioManager _twilioManager;
        protected APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProductController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, IMapper mapper, IContentRepository contentRepository, RoleManager<IdentityRole> roleManager, IEmailManager emailSender, IUploadRepository uploadRepository, ITwilioManager twilioManager)
        {
            _userRepo = userRepo;
            _response = new();
            _context = context;
            _uploadRepository = uploadRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _twilioManager = twilioManager;
            _userManager = userManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _roleManager = roleManager;
            _contentRepository = contentRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        #region AddProduct
        /// <summary>
        ///  AddProduct. 
        /// </summary>
        [HttpPost("AddProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddProduct([FromBody] ProductVariantDTO model)
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

            var mainCategory = await _context.MainCategory.FindAsync(model.MainCategoryId);
            if (mainCategory == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "mainCategory not found.";
                return Ok(_response);
            }
            var subCategory = await _context.SubCategory.FindAsync(model.SubCategoryId);
            if (subCategory == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "subCategory not found.";
                return Ok(_response);
            }
            var brand = await _context.Brand.FindAsync(model.BrandId);
            if (brand == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "brand not found.";
                return Ok(_response);
            }

            var product = new Product();
            _mapper.Map(model, product);
            _context.Add(product);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<ProductResponsesDTO>(product);

            if (model.Camera != null)
            {
                CameraVariants cameraVariants = new CameraVariants();
                cameraVariants.ProductId = product.ProductId;
                _mapper.Map(model.Camera, cameraVariants);
                _context.Add(cameraVariants);
                await _context.SaveChangesAsync();
                response.Camera = _mapper.Map<CameraVariantsDTO>(cameraVariants);
            }

            if (model.Audio != null)
            {
                var audioVariants = new AudioVariants();
                audioVariants.ProductId = product.ProductId;
                _mapper.Map(model.Audio, audioVariants);
                _context.Add(audioVariants);
                await _context.SaveChangesAsync();
                response.Camera = _mapper.Map<CameraVariantsDTO>(audioVariants);
            }

            if (model.Certification != null)
            {
                var certificationVariants = new CertificationVariants();
                certificationVariants.ProductId = product.ProductId;
                _mapper.Map(model.Audio, certificationVariants);
                _context.Add(certificationVariants);
                await _context.SaveChangesAsync();
                response.Certification = _mapper.Map<CertificationVariantsDTO>(certificationVariants);
            }

            if (model.Environment != null)
            {
                var environmentVariants = new EnvironmentVariants();
                environmentVariants.ProductId = product.ProductId;
                _mapper.Map(model.Environment, environmentVariants);
                await _context.SaveChangesAsync();
                _context.Add(environmentVariants);
                response.Environment = _mapper.Map<EnvironmentVariantsDTO>(environmentVariants);
            }

            if (model.General != null)
            {
                var generalVariants = new GeneralVariants();
                generalVariants.ProductId = product.ProductId;
                _mapper.Map(model.General, generalVariants);
                await _context.SaveChangesAsync();
                _context.Add(generalVariants);
                response.General = _mapper.Map<GeneralVariantsDTO>(generalVariants);

            }

            if (model.Lens != null)
            {
                var lensVariants = new LensVariants();
                lensVariants.ProductId = product.ProductId;
                _mapper.Map(model.Lens, lensVariants);
                await _context.SaveChangesAsync();
                _context.Add(lensVariants);
                response.Lens = _mapper.Map<LensVariantsDTO>(lensVariants);

            }

            if (model.Network != null)
            {
                var networkVariants = new NetworkVariants();
                networkVariants.ProductId = product.ProductId;
                _mapper.Map(model.Network, networkVariants);
                await _context.SaveChangesAsync();
                _context.Add(networkVariants);
                response.Network = _mapper.Map<NetworkVariantsDTO>(networkVariants);
            }

            if (model.Power != null)
            {
                var powerVariants = new PowerVariants();
                powerVariants.ProductId = product.ProductId;
                _mapper.Map(model.Power, powerVariants);
                await _context.SaveChangesAsync();
                _context.Add(powerVariants);
                response.Power = _mapper.Map<PowerVariantsDTO>(powerVariants);
            }

            if (model.Video != null)
            {
                var videoVariants = new VideoVariants();
                videoVariants.ProductId = product.ProductId;
                _mapper.Map(model.Video, videoVariants);
                await _context.SaveChangesAsync();
                _context.Add(videoVariants);
                response.Video = _mapper.Map<VideoVariantsDTO>(videoVariants);
            }

            response.CreateDate = product.CreateDate.ToString("dd-MM-yyyy");

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Product added successfully.";

            return Ok(_response);
        }
        #endregion

        #region UpdateProduct
        /// <summary>
        ///  UpdateProduct. 
        /// </summary>
        [HttpPost("UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateDTO model)
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
                _response.Messages = "Not found any user.";
                return Ok(_response);
            }

            var mainCategory = await _context.MainCategory.FindAsync(model.MainCategoryId);
            if (mainCategory == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Category not found.";
                return Ok(_response);
            }
            var subCategory = await _context.SubCategory.FindAsync(model.SubCategoryId);
            if (subCategory == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Category not found.";
                return Ok(_response);
            }
            var brand = await _context.Brand.FindAsync(model.BrandId);
            if (brand == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Brand not found.";
                return Ok(_response);
            }

            var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == model.ProductId && u.IsDeleted == false);
            if (product != null)
            {
                _mapper.Map(model, product);
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var cameraVariants = await _context.CameraVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (cameraVariants != null)
            {
                if (model.Camera != null)
                {
                    _mapper.Map(model.Camera, cameraVariants);
                    _context.Update(cameraVariants);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.Remove(cameraVariants);
                }
                await _context.SaveChangesAsync();

            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var audioVariants = await _context.AudioVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (audioVariants != null)
            {
                if (model.Audio != null)
                {
                    _mapper.Map(model.Audio, audioVariants);
                    _context.Update(audioVariants);
                }
                else
                {
                    _context.Remove(audioVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var certificationVariants = await _context.CertificationVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (certificationVariants != null)
            {

                if (model.Certification != null)
                {
                    _mapper.Map(model.Certification, certificationVariants);
                    _context.Update(certificationVariants);
                }
                else
                {
                    _context.Remove(certificationVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var environmentVariants = await _context.EnvironmentVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (environmentVariants != null)
            {
                if (model.Environment != null)
                {
                    _mapper.Map(model.Environment, environmentVariants);
                    _context.Update(environmentVariants);
                }
                else
                {
                    _context.Remove(environmentVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var generalVariants = await _context.GeneralVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (generalVariants != null)
            {
                if (model.General != null)
                {
                    _mapper.Map(model.General, generalVariants);
                    _context.Update(generalVariants);
                }
                else
                {
                    _context.Remove(generalVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var lensVariants = await _context.LensVariants.FirstOrDefaultAsync(u => u.ProductId == cameraVariants.VariantId);
            if (lensVariants != null)
            {
                if (model.Lens != null)
                {
                    _mapper.Map(model.Lens, lensVariants);
                    _context.Update(lensVariants);
                }
                else
                {
                    _context.Remove(lensVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var networkVariants = await _context.NetworkVariants.FirstOrDefaultAsync(u => u.ProductId == model.ProductId);
            if (networkVariants != null)
            {
                if (model.Network != null)
                {
                    _mapper.Map(model.Network, networkVariants);
                    _context.Update(networkVariants);
                }
                else
                {
                    _context.Remove(networkVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var powerVariants = await _context.PowerVariants.FirstOrDefaultAsync(u => u.ProductId == cameraVariants.VariantId);
            if (powerVariants != null)
            {
                if (model.Power != null)
                {
                    _mapper.Map(model.Power, powerVariants);
                    _context.Update(powerVariants);
                }
                else
                {
                    _context.Remove(powerVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var videoVariants = await _context.VideoVariants.FirstOrDefaultAsync(u => u.ProductId == cameraVariants.VariantId);
            if (videoVariants != null)
            {
                if (model.Video != null)
                {
                    _mapper.Map(model.Video, videoVariants);
                    _context.Update(videoVariants);
                }
                else
                {
                    _context.Remove(videoVariants);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = model;
            _response.Messages = "Product Updated successfully.";
            return Ok(_response);
        }
        #endregion

        #region GetProductList
        /// <summary>
        ///  Get product list.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("GetProductList")]
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
            var roles = await _userManager.GetRolesAsync(currentUserDetail);

            if (roles.FirstOrDefault() == "Distributor")
            {
                var distributor = _context.DistributorDetail.Where(u => u.UserId == currentUserId).FirstOrDefault();
                if (distributor.Status != Status.Approved.ToString())
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor is not approved.";
                    return Ok(_response);
                }
            }

            var query = from t1 in _context.Product
                        where !t1.IsDeleted
                        orderby t1.CreateDate descending
                        select new ProductResponselistDTO
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
                            Discount = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? t1.Discount : t1.DistributorDiscount),
                            DiscountType = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? t1.DiscountType : t1.DistributorDiscountType),
                            SellingPrice = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? t1.SellingPrice : t1.DistributorSellingPrice),
                            DistributorDiscount = (roles.FirstOrDefault() == "Admin" ? t1.DistributorDiscount : 0),
                            DistributorDiscountType = (roles.FirstOrDefault() == "Admin" ? t1.DistributorDiscountType : 0),
                            DistributorSellingPrice = (roles.FirstOrDefault() == "Admin" ? t1.DistributorSellingPrice : 0),
                            RewardPoint = _context.ProductStock.Where(u => u.ProductId == t1.ProductId).FirstOrDefault().RewardPoint,
                            InStock = _context.ProductStock.Where(u => u.ProductId == t1.ProductId).ToList().Count,
                            CreateDate = t1.CreateDate.ToShortDateString()
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
            _response.Messages = "Product" + ResponseMessages.msgListFoundSuccess;
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

            var lensVariants = await _context.LensVariants.FirstOrDefaultAsync(u => u.VariantId == productId);

            var networkVariants = await _context.NetworkVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var powerVariants = await _context.PowerVariants.FirstOrDefaultAsync(u => u.VariantId == productId);

            var videoVariants = await _context.VideoVariants.FirstOrDefaultAsync(u => u.VariantId == productId);

            // var accessoriesVariants = await _context.AccessoriesVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var userMannual = await _context.UserManual.Where(u => u.ProductId == productId).ToListAsync();

            var installationDocumentVariants = await _context.InstallationDocumentVariants.Where(u => u.ProductId == productId).ToListAsync();

            var response = _mapper.Map<ProductResponsesDTO>(product);
            response.CreateDate = product.CreateDate.ToString("dd-MM-yyyy");

            response.Discount = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? response.Discount : response.DistributorDiscount);
            response.DiscountType = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? response.DiscountType : response.DistributorDiscountType);
            response.SellingPrice = (roles.FirstOrDefault() == "Dealer" || roles.FirstOrDefault() == "Admin" ? response.SellingPrice : response.DistributorSellingPrice);
            response.DistributorDiscount = (roles.FirstOrDefault() == "Admin" ? response.DistributorDiscount : 0);
            response.DistributorDiscountType = (roles.FirstOrDefault() == "Admin" ? response.DistributorDiscountType : 0);
            response.DistributorSellingPrice = (roles.FirstOrDefault() == "Admin" ? response.DistributorSellingPrice : 0);

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
            response.Audio = _mapper.Map<AudioVariantsDTO>(audioVariants);
            response.Camera = _mapper.Map<CameraVariantsDTO>(cameraVariants);
            response.Certification = _mapper.Map<CertificationVariantsDTO>(certificationVariants);
            response.Environment = _mapper.Map<EnvironmentVariantsDTO>(environmentVariants);
            response.General = _mapper.Map<GeneralVariantsDTO>(generalVariants);
            response.Lens = _mapper.Map<LensVariantsDTO>(lensVariants);
            response.Network = _mapper.Map<NetworkVariantsDTO>(networkVariants);
            response.Power = _mapper.Map<PowerVariantsDTO>(powerVariants);
            response.Video = _mapper.Map<VideoVariantsDTO>(videoVariants);
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

        #region DeleteProduct
        /// <summary>
        ///  Delete product.
        /// </summary>
        [HttpDelete("DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(u => u.ProductId == productId && !u.IsDeleted);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return NotFound(_response);
            }
            var cart = await _context.CartDetail
                .Where(u => u.ProductId == productId).ToListAsync();

            _context.RemoveRange(cart);
            await _context.SaveChangesAsync();

            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return NotFound(_response);
            }

            product.IsDeleted = true;

            _context.Update(product);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Product deleted successfully.";
            return Ok(_response);

        }

        #endregion

        #region SetProductStatus
        /// <summary>
        ///  Product Status.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("SetProductStatus")]
        public async Task<IActionResult> SetProductStatus([FromBody] SetProductStatusDTO model)
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


            var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == model.productId && u.IsDeleted == false);
            if (product == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }

            product.IsActive = model.IsActive;

            _context.Update(product);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Product status updated successfully.";

            return Ok(_response);
        }
        #endregion

        #region AddProductSerialNo
        /// <summary>
        ///  AddProductSerialNo. 
        /// </summary>
        [HttpPost("AddProductSerialNo")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddProductSerialNo(AddProductQRAndSerialDTO model)
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

            var existingProduct = await _context.Product.Where(u => u.ProductId == model.ProductId && u.IsDeleted != true).FirstOrDefaultAsync();
            if (existingProduct == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any product.";
                return Ok(_response);
            }

            var existingProductStock = await _context.ProductStock
                .Where(ps => ps.SerialNumber == model.SerialNumber)
                .FirstOrDefaultAsync();
            model.SerialNumber = model.SerialNumber.ToUpper();
            int rewardPoint = 0;

            if (existingProductStock == null)
            {
                existingProductStock = new ProductStock();
                ProductStock productStock = new ProductStock();
                string input = model.SerialNumber; // Change this to your input string
                if (input.Length >= 2 && Char.IsLetter(input[0]) && Char.IsLetter(input[1]))
                {
                    char first = Char.ToUpper(input[0]);
                    char second = Char.ToUpper(input[1]);

                    if (first <= 'J' && second <= 'J')
                    {
                        int firstValue = first - 'A';
                        int secondValue = second - 'A';

                        string result = String.Format("{0:D2}", firstValue * 10 + secondValue);
                        rewardPoint = Convert.ToInt32(result);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "The first two letters should be between A and J..";
                        return Ok(_response);
                    }
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Input should contain at least two alphabetical characters.";
                    return Ok(_response);
                }

                productStock.ProductId = model.ProductId;
                productStock.SerialNumber = model.SerialNumber;
                productStock.RewardPoint = rewardPoint;
                productStock.Status = SerialNumberStatus.Active.ToString();
                await _context.AddAsync(productStock);
                await _context.SaveChangesAsync();

                var responseDTO = _mapper.Map<ProductStockResponseDTO>(productStock);
                responseDTO.modifyDate = existingProductStock.ModifyDate.ToString();
                responseDTO.createDate = existingProductStock.CreateDate.ToShortDateString();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = responseDTO;
                _response.Messages = "Product serial number added successfully.";

                return Ok(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "This serial number is already added.";

                return Ok(_response);
            }


        }
        #endregion

        #region UpdateProductSerialNo
        /// <summary>
        ///  UpdateProductSerialNo. 
        /// </summary>
        [HttpPost("UpdateProductSerialNo")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> UpdateProductSerialNo(UpdateProductQRAndSerialDTO model)
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

            var existingProduct = await _context.Product.Where(u => u.ProductId == model.ProductId && u.IsDeleted != true).FirstOrDefaultAsync();
            if (existingProduct == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any product.";
                return Ok(_response);
            }

            var existingProductStock = await _context.ProductStock
                .Where(ps => ps.SerialNumber == model.SerialNumber && ps.ProductStockId != model.ProductStockId)
                .FirstOrDefaultAsync();

            var productStock = await _context.ProductStock.Where(ps => ps.ProductStockId != model.ProductStockId).FirstOrDefaultAsync();

            model.SerialNumber = model.SerialNumber.ToUpper();
            int rewardPoint = 0;

            if (existingProductStock == null && productStock != null)
            {
                string input = model.SerialNumber; // Change this to your input string
                if (input.Length >= 2 && Char.IsLetter(input[0]) && Char.IsLetter(input[1]))
                {
                    char first = Char.ToUpper(input[0]);
                    char second = Char.ToUpper(input[1]);

                    if (first <= 'J' && second <= 'J')
                    {
                        int firstValue = first - 'A';
                        int secondValue = second - 'A';

                        string result = String.Format("{0:D2}", firstValue * 10 + secondValue);
                        rewardPoint = Convert.ToInt32(result);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "The first two letters should be between A and J..";
                        return Ok(_response);
                    }
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Input should contain at least two alphabetical characters.";
                    return Ok(_response);
                }

                productStock.ProductId = model.ProductId;
                productStock.SerialNumber = model.SerialNumber;
                productStock.RewardPoint = rewardPoint;
                productStock.Status = model.Status;

                _context.Update(productStock);
                await _context.SaveChangesAsync();

                var responseDTO = _mapper.Map<ProductStockResponseDTO>(productStock);
                responseDTO.modifyDate = existingProductStock.ModifyDate.ToString();
                responseDTO.createDate = existingProductStock.CreateDate.ToShortDateString();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = responseDTO;
                _response.Messages = "Product serial number updated successfully.";

                return Ok(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "This serial number added with another product.";

                return Ok(_response);
            }


        }
        #endregion

        #region GetProductSerialNumberList
        /// <summary>
        ///  GetProductQRAndSerialList. 
        /// </summary>
        [HttpGet("GetProductSerialNumberList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> GetProductSerialNumberList(int productId, string? status)
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

            var existingProduct = await _context.Product.Where(u => u.ProductId == productId && u.IsDeleted != true).FirstOrDefaultAsync();
            if (existingProduct == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any product.";
                return Ok(_response);
            }

            var productStockList = await _context.ProductStock.Where(ps => ps.ProductId == productId && ps.Status == SerialNumberStatus.Active.ToString()).ToListAsync();

            var responseDTO = productStockList
                .Select(p => new ProductStockResponseDTO
                {
                    productStockId = p.ProductStockId,
                    productId = p.ProductId,
                    serialNumber = p.SerialNumber,
                    createDate = p.CreateDate.ToString(DefaultDateFormat),
                    status = p.Status,
                })
                .ToList();

            if (!string.IsNullOrEmpty(status))
            {
                if (status != SerialNumberStatus.Active.ToString() && status != SerialNumberStatus.Scanned.ToString())
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Please enter valid status.";
                    return Ok(_response);
                }
                responseDTO = responseDTO.Where(u => u.status == status).ToList();
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = responseDTO;
            _response.Messages = "List shown successfully.";

            return Ok(_response);
        }
        #endregion

        #region RedeemProduct
        /// <summary>
        ///  RedeemProduct.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Dealer,Distributor")]
        [Route("RedeemProduct")]
        public async Task<IActionResult> RedeemProduct(RedeemProductRequestDTO model)
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

            var rewardProductDetail = _context.RewardProduct.Where(u => u.RewardProductId == model.rewardProductId).FirstOrDefault();
            if (rewardProductDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            else
            {
                if (rewardProductDetail.Stock < model.productCount)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Stock is unavailable.";
                    return Ok(_response);
                }
            }

            var redeemProduct = new RedeemedProducts();

            var roles = await _userManager.GetRolesAsync(currentUserDetail);
            var roleName = roles.FirstOrDefault();
            if (roleName == "Dealer")
            {
                var dealerDetail = await _context.DealerDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                var poitDetail = _context.Points.Where(u => u.UserId == dealerDetail.UserId).FirstOrDefault();

                if (poitDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Not enough points to redeem.";
                    return Ok(_response);
                }

                if (poitDetail.ActivePoints < (rewardProductDetail.NeededPointToRedeem * model.productCount))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Not enough points to redeem.";
                    return Ok(_response);
                }
                redeemProduct.RewardProductId = model.rewardProductId;
                redeemProduct.ProductCount = model.productCount;
                redeemProduct.Status = Status.Pending.ToString();
                redeemProduct.UserId = dealerDetail.UserId;
                redeemProduct.ReedemedPoint = rewardProductDetail.NeededPointToRedeem * model.productCount;

                poitDetail.ActivePoints = poitDetail.ActivePoints - (rewardProductDetail.NeededPointToRedeem * model.productCount);
                poitDetail.RedeemedPoints = poitDetail.RedeemedPoints + (rewardProductDetail.NeededPointToRedeem * model.productCount);

                _context.AddAsync(redeemProduct);
                _context.SaveChanges();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Product redeemed successfully.";
                return Ok(_response);
            }
            else if (roleName == "Distributor")
            {
                var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                var poitDetail = _context.Points.Where(u => u.UserId == distributorDetail.UserId).FirstOrDefault();

                if (poitDetail.ActivePoints < (rewardProductDetail.NeededPointToRedeem * model.productCount))
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Not enough points to redeem.";
                    return Ok(_response);
                }
                redeemProduct.RewardProductId = model.rewardProductId;
                redeemProduct.ProductCount = model.productCount;
                redeemProduct.Status = Status.Pending.ToString();
                redeemProduct.UserId = distributorDetail.UserId;
                redeemProduct.ReedemedPoint = rewardProductDetail.NeededPointToRedeem * model.productCount;

                poitDetail.ActivePoints = poitDetail.ActivePoints - (rewardProductDetail.NeededPointToRedeem * model.productCount);
                poitDetail.RedeemedPoints = poitDetail.RedeemedPoints + (rewardProductDetail.NeededPointToRedeem * model.productCount);

                _context.AddAsync(redeemProduct);
                _context.SaveChanges();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Product redeemed successfully.";
                return Ok(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
        }
        #endregion

        #region GetRedeemProductList
        /// <summary>
        ///  GetRedeemProductList. 
        /// </summary>
        [HttpGet("GetRedeemProductList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> GetRedeemProductList(int? dealerId, int? distributorId, string? status)
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

            var roles = await _userManager.GetRolesAsync(existingUser);
            var roleName = roles.FirstOrDefault();
            string userId = "";

            DealerDetail? dealerDetail = null;
            DistributorDetail? distributorDetail = null;
            List<RedeemedProductDTO>? redeemedProducts = null;

            if (roleName != "Dealer" && roleName != "Distributor")
            {
                if (dealerId == null && distributorId == null)
                {
                    redeemedProducts = (from rp in _context.RedeemedProducts
                                        join rewardProduct in _context.RewardProduct on rp.RewardProductId equals rewardProduct.RewardProductId
                                        select new RedeemedProductDTO
                                        {
                                            RewardProductId = rp.RewardProductId,
                                            Name = rewardProduct.Name,
                                            Description = rewardProduct.Description,
                                            Image = rewardProduct.Image,
                                            productCount = (int)rp.ProductCount,
                                            MRP = (float)rewardProduct.Mrp,
                                            NeededPointToRedeem = rewardProduct.NeededPointToRedeem,
                                            status = rp.Status,
                                            redeemedDate = rp.CreateDate.ToString(DefaultDateFormat),
                                            ReedemProductId = rp.ReedemProductId // Assuming redeemedDate is a DateTime
                                        }).ToList();

                    if (!string.IsNullOrEmpty(status))
                    {
                        redeemedProducts = redeemedProducts.Where(u => u.status == status).ToList();
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = redeemedProducts;
                    _response.Messages = "List shown successfully.";

                    return Ok(_response);
                }
                else
                {
                    if (dealerId != null)
                    {
                        dealerDetail = _context.DealerDetail.Where(u => u.DealerId == dealerId).FirstOrDefault();
                        if (dealerDetail == null)
                        {
                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = false;
                            _response.Messages = "Not found any record.";
                            return Ok(_response);
                        }
                        dealerId = dealerDetail.DealerId;
                        userId = dealerDetail.UserId;
                    }
                    else
                    {
                        distributorDetail = _context.DistributorDetail.Where(u => u.DistributorId == distributorId).FirstOrDefault();
                        if (dealerDetail == null)
                        {
                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = false;
                            _response.Messages = "Not found any record.";
                            return Ok(_response);
                        }
                        distributorId = distributorDetail.DistributorId;
                        userId = distributorDetail.UserId;
                    }
                }
            }
            else
            {
                if (roleName == "Dealer")
                {
                    dealerDetail = _context.DealerDetail.Where(u => u.UserId == currentUserId).FirstOrDefault();
                    dealerId = dealerDetail.DealerId;
                    userId = dealerDetail.UserId;
                }
                else
                {
                    distributorDetail = _context.DistributorDetail.Where(u => u.UserId == currentUserId).FirstOrDefault();
                    distributorId = distributorDetail.DistributorId;
                    userId = distributorDetail.UserId;
                }
            }

            var redeem = new RedeemedProductDTO();

            // Assuming _context is your database context

            // LINQ query to retrieve RedeemedProducts for a specific UserId
            redeemedProducts = (from rp in _context.RedeemedProducts
                                join rewardProduct in _context.RewardProduct on rp.RewardProductId equals rewardProduct.RewardProductId
                                where rp.UserId == userId
                                select new RedeemedProductDTO
                                {
                                    RewardProductId = rp.RewardProductId,
                                    Name = rewardProduct.Name,
                                    Description = rewardProduct.Description,
                                    Image = rewardProduct.Image,
                                    productCount = (int)rp.ProductCount,
                                    MRP = (float)rewardProduct.Mrp,
                                    NeededPointToRedeem = rewardProduct.NeededPointToRedeem,
                                    status = rp.Status,
                                    redeemedDate = rp.CreateDate.ToString(DefaultDateFormat),
                                    ReedemProductId = rp.ReedemProductId // Assuming redeemedDate is a DateTime
                                }).ToList();

            if (!string.IsNullOrEmpty(status))
            {
                redeemedProducts = redeemedProducts.Where(u => u.status == status).ToList();
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = redeemedProducts;
            _response.Messages = "List shown successfully.";

            return Ok(_response);
        }
        #endregion

        #region SetRedeemProductStatus
        /// <summary>
        ///  Set Redeem Product Status. 
        /// </summary>
        [HttpPost("SetRedeemProductStatus")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> SetRedeemProductStatus(RedeemedProductStatusDTO model)
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

            if (model.status != "Approved" && model.status != "Rejected" && model.status != "Pending")
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid status.";
                return Ok(_response);
            }
            var redeemedProducts = await _context.RedeemedProducts.Where(u => u.ReedemProductId == model.ReedemProductId).FirstOrDefaultAsync();
            if (redeemedProducts == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "not found any product.";
                return Ok(_response);
            }
            else
            {
                redeemedProducts.Status = model.status;
                if (model.status == "Rejected")
                {
                    var point = await _context.Points.Where(u => u.UserId == redeemedProducts.UserId).FirstOrDefaultAsync();
                    if (point != null)
                    {
                        var rewardProduct = await _context.RewardProduct.Where(u => u.RewardProductId == redeemedProducts.RewardProductId).FirstOrDefaultAsync();

                        if (rewardProduct != null)
                        {
                            point.ActivePoints = point.ActivePoints + rewardProduct.NeededPointToRedeem * redeemedProducts.ProductCount;

                            point.RedeemedPoints = point.RedeemedPoints - rewardProduct.NeededPointToRedeem * redeemedProducts.ProductCount;
                        }
                    }
                }
                _context.Update(redeemedProducts);
                _context.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Staus updated successfully.";
                return Ok(_response);
            }

        }
        #endregion
    }
}
