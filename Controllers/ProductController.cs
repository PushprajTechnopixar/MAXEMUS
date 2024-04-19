﻿using AutoMapper;
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


            CameraVariants cameraVariants = new CameraVariants();
            foreach (var item in model.Camera)
            {
                cameraVariants.ProductId = product.ProductId;
                cameraVariants.Appearance = item.Appearance;

                _mapper.Map(item, cameraVariants);
                _context.Add(cameraVariants);
                await _context.SaveChangesAsync();
            }

            var audioVariants = new AudioVariants();
            foreach (var item in model.Audio)
            {
                audioVariants.ProductId = product.ProductId;
                audioVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, audioVariants);
                _context.Add(audioVariants);
                await _context.SaveChangesAsync();

            }

            var certificationVariants = new CertificationVariants();
            foreach (var item in model.Certification)
            {
                certificationVariants.ProductId = product.ProductId;
                certificationVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, certificationVariants);
                _context.Add(certificationVariants);
                await _context.SaveChangesAsync();

            }

            var environmentVariants = new EnvironmentVariants();
            foreach (var item in model.Environment)
            {
                environmentVariants.ProductId = product.ProductId;
                environmentVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, environmentVariants);
                await _context.SaveChangesAsync();
                _context.Add(environmentVariants);
            }

            var generalVariants = new GeneralVariants();
            foreach (var item in model.General)
            {
                generalVariants.ProductId = product.ProductId;
                generalVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, generalVariants);
                await _context.SaveChangesAsync();
                _context.Add(generalVariants);
            }

            var lensVariants = new LensVariants();
            foreach (var item in model.Lens)
            {
                lensVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, lensVariants);
                await _context.SaveChangesAsync();
                _context.Add(lensVariants);

            }

            var networkVariants = new NetworkVariants();
            foreach (var item in model.Network)
            {
                networkVariants.ProductId = product.ProductId;
                networkVariants.VariantId = cameraVariants.VariantId;


                _mapper.Map(item, networkVariants);
                await _context.SaveChangesAsync();
                _context.Add(networkVariants);
            }

            var powerVariants = new PowerVariants();
            foreach (var item in model.Power)
            {
                powerVariants.VariantId = cameraVariants.VariantId;


                _mapper.Map(item, powerVariants);
                await _context.SaveChangesAsync();
                _context.Add(powerVariants);
            }

            var videoVariants = new VideoVariants();
            foreach (var item in model.Video)
            {
                videoVariants.VariantId = cameraVariants.VariantId;

                _mapper.Map(item, videoVariants);
                await _context.SaveChangesAsync();
                _context.Add(videoVariants);
            }


            var accessoriesVariants = new AccessoriesVariants
            {
                ProductId = product.ProductId,
            };
            _mapper.Map(model, accessoriesVariants);
            _context.Add(accessoriesVariants);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<ProductResponsesDTO>(product);
            response.CreateDate = product.CreateDate.ToString("dd-MM-yyyy");
            response.VariantId = cameraVariants.VariantId;
            response.Accessories = _mapper.Map<AccessoriesVariantsDTO>(accessoriesVariants);
            response.Audio = _mapper.Map<AudioVariantsDTO>(model.Audio.FirstOrDefault());
            response.Camera = _mapper.Map<CameraVariantsDTO>(model.Camera.FirstOrDefault());
            response.Certification = _mapper.Map<CertificationVariantsDTO>(model.Certification.FirstOrDefault());
            response.Environment = _mapper.Map<EnvironmentVariantsDTO>(model.Environment.FirstOrDefault());
            response.General = _mapper.Map<GeneralVariantsDTO>(model.General.FirstOrDefault());

            response.Lens = _mapper.Map<LensVariantsDTO>(model.Lens.FirstOrDefault());
            response.Network = _mapper.Map<NetworkVariantsDTO>(model.Network.FirstOrDefault());
            response.Power = _mapper.Map<PowerVariantsDTO>(model.Power.FirstOrDefault());
            response.Video = _mapper.Map<VideoVariantsDTO>(model.Video.FirstOrDefault());


            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Product added successfully.";

            return Ok(_response);


        }
        #endregion

        #region AddProductToStock
        /// <summary>
        ///  AddProductToStock. 
        /// </summary>
        [HttpPost("AddProductToStock")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddProductToStock([FromForm] AddQR model)
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
                .Where(ps => ps.ProductId == model.ProductId && ps.SerialNumber == model.SerialNumber)
                .FirstOrDefaultAsync();
            model.SerialNumber = model.SerialNumber.ToUpper();
            int rewardPoint = 0;

            if (existingProductStock == null)
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

                var qrcodeFileName = ContentDispositionHeaderValue.Parse(model.Qrcode.ContentDisposition).FileName.Trim('"');
                qrcodeFileName = CommonMethod.EnsureCorrectFilename(qrcodeFileName);
                qrcodeFileName = CommonMethod.RenameFileName(qrcodeFileName);

                var qrcodePath = qrCodeContainer + qrcodeFileName;

                existingProductStock.ModifyDate = DateTime.UtcNow;
                existingProductStock.Qrcode = qrcodePath;

                _context.ProductStock.Update(existingProductStock);
                await _context.SaveChangesAsync();

                bool uploadStatus = await _uploadRepository.UploadFilesToServer(
                    model.Qrcode,
                    qrCodeContainer,
                    qrcodeFileName
                );

                var responseDTO = _mapper.Map<ProductStockResponseDTO>(existingProductStock);
                responseDTO.modifyDate = existingProductStock.ModifyDate.ToString();
                responseDTO.createDate = existingProductStock.CreateDate.ToShortDateString();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = responseDTO;
                _response.Messages = "Product stock updated successfully.";

                return Ok(_response);
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Already added.";

                return Ok(_response);
            }


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
                _mapper.Map(model.Camera, cameraVariants);
                _context.Update(cameraVariants);
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
                _mapper.Map(model.Audio, audioVariants);
                _context.Update(audioVariants);
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

                _mapper.Map(model.Certification, certificationVariants);
                _context.Update(certificationVariants);
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
                _mapper.Map(model.Environment, environmentVariants);
                _context.Update(environmentVariants);
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
                _mapper.Map(model.General, generalVariants);
                _context.Update(generalVariants);
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var lensVariants = await _context.LensVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);
            if (lensVariants != null)
            {
                _mapper.Map(model.Lens, lensVariants);
                _context.Update(lensVariants);
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
                _mapper.Map(model.Network, networkVariants);
                _context.Update(networkVariants);
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var powerVariants = await _context.PowerVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);
            if (powerVariants != null)
            {
                _mapper.Map(model.Power, powerVariants);
                _context.Update(powerVariants);
                await _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var videoVariants = await _context.VideoVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);
            if (videoVariants != null)
            {
                _mapper.Map(model.Video, videoVariants);
                _context.Update(videoVariants);
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
                            TotalMrp = (roles.FirstOrDefault() != "Dealer" ? t1.TotalMrp : 0),
                            Discount = (roles.FirstOrDefault() != "Dealer" ? t1.Discount : 0),
                            DiscountType = (roles.FirstOrDefault() != "Dealer" ? t1.DiscountType : 0),
                            SellingPrice = (roles.FirstOrDefault() != "Dealer" ? t1.SellingPrice : 0),
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
            _response.Messages = "Item" + ResponseMessages.msgListFoundSuccess;
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

            var lensVariants = await _context.LensVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);

            var networkVariants = await _context.NetworkVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var powerVariants = await _context.PowerVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);

            var videoVariants = await _context.VideoVariants.FirstOrDefaultAsync(u => u.VariantId == cameraVariants.VariantId);

            var accessoriesVariants = await _context.AccessoriesVariants.FirstOrDefaultAsync(u => u.ProductId == productId);

            var response = _mapper.Map<ProductResponsesDTO>(product);
            response.CreateDate = product.CreateDate.ToString("dd-MM-yyyy");
            response.VariantId = cameraVariants.VariantId;


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
            response.RewardPoint = _context.ProductStock.Where(u => u.ProductId == product.ProductId).FirstOrDefault().RewardPoint;
            response.ProductImage = productImageList;
            response.Accessories = _mapper.Map<AccessoriesVariantsDTO>(accessoriesVariants);
            response.Audio = _mapper.Map<AudioVariantsDTO>(audioVariants);
            response.Camera = _mapper.Map<CameraVariantsDTO>(cameraVariants);
            response.Certification = _mapper.Map<CertificationVariantsDTO>(certificationVariants);
            response.Environment = _mapper.Map<EnvironmentVariantsDTO>(environmentVariants);
            response.General = _mapper.Map<GeneralVariantsDTO>(generalVariants);
            response.Lens = _mapper.Map<LensVariantsDTO>(lensVariants);
            response.Network = _mapper.Map<NetworkVariantsDTO>(networkVariants);
            response.Power = _mapper.Map<PowerVariantsDTO>(powerVariants);
            response.Video = _mapper.Map<VideoVariantsDTO>(videoVariants);

            if (roles.FirstOrDefault() == "Dealer")
            {
                response.TotalMrp = 0;
                response.Discount = 0;
                response.DiscountType = 0;
                response.SellingPrice = 0;
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

            product.IsActive = model.status;

            _context.Update(product);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Product status updated successfully.";

            return Ok(_response);
        }

        #endregion


    }
}
