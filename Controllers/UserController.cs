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
using static Google.Apis.Requests.BatchRequest;
using TimeZoneConverter;
using System.Linq;
using System.Web.Helpers;
using GSF.Net.Smtp;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
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

        public UserController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
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


        #region CreateUser
        [HttpPost]
        [Route("CreateUser")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO model)
        {
            bool ifUserNameUnique = _userRepo.IsUniqueEmail(model.email);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Email already exists.";
                return Ok(_response);
            }

            ApplicationUser user = new()
            {
                Email = model.email,
                UserName = model.email,
                NormalizedEmail = model.email.ToUpper(),
            };

            string role = "User";
            user.FirstName = "";
            user.LastName = "";
            var result = await _userManager.CreateAsync(user, model.password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                var userDetail = new UserDetail();
                userDetail.UserId = user.Id;
                userDetail.Status = "Approved";

                _context.Add(userDetail);
                _context.SaveChangesAsync();
            }
            else
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Error while registering.";
                return Ok(_response);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim("SecurityStamp", user.SecurityStamp),
                    // new Claim(ClaimTypes.Anonymous,user.SecurityStamp)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                token = tokenHandler.WriteToken(token),
            };
            _mapper.Map(user, loginResponseDTO);

            var userdetail = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userdetail == null)
            {
                loginResponseDTO = new LoginResponseDTO();
            }
            loginResponseDTO.role = "User";

            loginResponseDTO.gender = userdetail.Gender;
            loginResponseDTO.dialCode = userdetail.DialCode;

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = loginResponseDTO;
            _response.Messages = "Registered successfully.";
            return Ok(_response);

        }
        #endregion

        #region GetProfileDetail
        /// <summary>
        ///  Get profile.
        /// </summary>
        [HttpGet("GetProfileDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetProfileDetail(string? userId)
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
            UserDetail? user = null;
            user = await _context.UserDetail.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (user == null)
            {
                user = await _context.UserDetail.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            }
            var mappedData = _mapper.Map<UserDetailResponseDTO>(user);
            _mapper.Map(user, mappedData);
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
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDetailRequestDTO model)
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
            if (userDetail.PhoneNumber != null)
            {
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
            }
            if (Gender.Male.ToString() != model.gender && Gender.Female.ToString() != model.gender && Gender.Other.ToString() != model.gender)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid gender.";
                return Ok(_response);
            }

            var user = await _context.UserDetail.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();

            if (user == null)
            {
                user = new UserDetail
                {
                    UserId = currentUserId,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    CreateDate = DateTime.UtcNow,
                    Status = Status.Pending.ToString(),
                };

                _context.Add(user);
            }
            else
            {
                user.Address1 = model.Address1;
                user.Address2 = model.Address2;
                user.ModifyDate = DateTime.UtcNow;
                user.Status = Status.Approved.ToString();
                _context.Update(user);
            }

            await _context.SaveChangesAsync();

            var mappedData = _mapper.Map(model, userDetail);
            _context.Update(userDetail);
            await _context.SaveChangesAsync();

            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(model, userProfileDetail);
            _context.ApplicationUsers.Update(updateProfile);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<UserDetailResponseDTO>(user);
            _mapper.Map(userProfileDetail, response);

            var userCountry = await _context.CountryMaster.Where(u => u.CountryId == response.countryId).FirstOrDefaultAsync();
            var userState = await _context.StateMaster.Where(u => u.StateId == response.stateId).FirstOrDefaultAsync();

            if (userCountry != null && userState != null)
            {
                response.CountryName = userCountry.CountryName;
                response.StateName = userState.StateName;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Profile updated successfully.";
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
        [AllowAnonymous]
        [Route("GetProductDetail")]
        public async Task<IActionResult> GetProductDetail(int productId)
        {
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

            var response = _mapper.Map<UserProductResponsesDTO>(product);
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

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Product Detail shown successfully.";

            return Ok(_response);
        }
        #endregion

        #region AddProductToCart
        /// <summary>
        ///  Add product to cart.
        /// </summary>
        [HttpPost("AddProductToCart")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductToCartDTO model)
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

                var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == model.ProductId && u.IsActive == true);
                if (product == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                var user = await _context.UserDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                // var productStock = await _context.ProductStock.Where(u => u.ProductId == model.ProductId).ToListAsync();
                // if (productStock.Count < 1)
                // {
                //     _response.StatusCode = HttpStatusCode.NotFound;
                //     _response.IsSuccess = false;
                //     _response.Messages = "product is out of stock.";
                //     return Ok(_response);
                // }

                var cart = await _context.CartDetail.FirstOrDefaultAsync(c => c.ProductId == model.ProductId && c.DistributorId == user.UserDetailId);
                if (cart == null)
                {
                    cart = new CartDetail
                    {
                        ProductId = model.ProductId,
                        UserDetailId = user.UserDetailId,
                        ProductCountInCart = model.ProductCountInCart,
                        CreateDate = DateTime.UtcNow,
                    };
                    _context.CartDetail.Add(cart);
                }
                else
                {
                    cart.ProductCountInCart = model.ProductCountInCart;
                    if (model.ProductCountInCart > 0)
                    {
                        _context.CartDetail.Update(cart);
                    }
                    else
                    {
                        _context.CartDetail.Remove(cart);
                    }
                }

                await _context.SaveChangesAsync();

                var response = _mapper.Map<CartResponseDTO>(cart);
                _mapper.Map(product, response);
                response.CreateDate = cart.CreateDate.ToString("dd-MM-yyyy");
                response.TotalMrp = (double)(product.TotalMrp * cart.ProductCountInCart);
                response.DistributorDiscount = (double)(product.DistributorDiscount * cart.ProductCountInCart);
                response.DistributorSellingPrice = (double)(product.DistributorSellingPrice * cart.ProductCountInCart);
                response.DistributorDiscountType = product.DistributorDiscountType;

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = response;
                _response.Messages = "Product added to cart successfully.";

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

        #region GetProductListFromCart
        /// <summary>
        ///  Get product list from cart.
        /// </summary>
        [HttpGet("GetProductListFromCart")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetProductListFromCart()
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
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgUserNotFound;
                    return NotFound(_response);
                }

                var user = await _context.UserDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                var cart = await _context.CartDetail.Where(u => u.UserDetailId == user.UserDetailId).ToListAsync();
                if (cart.Count == 0)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                UserCartDetailDTO cartDetail = new UserCartDetailDTO();
                var mappedData = new List<UserProductListFromCart>();

                // Calculate totals
                int totalItem = 0;
                double totalMrp = 0;
                double totalSellingPrice = 0;
                double totalDiscountAmount = 0;

                foreach (var item in cart)
                {
                    var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);
                    if (product != null)
                    {
                        var productResponse = _mapper.Map<UserProductListFromCart>(product);
                        productResponse.ProductCountInCart = item.ProductCountInCart;
                        productResponse.TotalMrp = (double)(product.TotalMrp * item.ProductCountInCart);
                        productResponse.Discount = (double)(product.Discount * item.ProductCountInCart);
                        productResponse.DiscountType = product.DiscountType;
                        productResponse.SellingPrice = ((double)(productResponse.SellingPrice * item.ProductCountInCart));

                        mappedData.Add(productResponse);

                        // Update totals
                        totalItem++;
                        totalMrp += productResponse.TotalMrp;
                        totalSellingPrice += productResponse.SellingPrice;
                        totalDiscountAmount += productResponse.Discount;
                    }
                }

                // Populate CartDetailDTO

                cartDetail.totalItem = totalItem;
                cartDetail.totalMrp = Math.Round(totalMrp, 2);
                cartDetail.totalSellingPrice = Math.Round(totalSellingPrice, 2);
                cartDetail.totalDiscountAmount = Math.Round(totalDiscountAmount, 2);
                cartDetail.totalDiscount = totalMrp != 0 ? Math.Round((totalDiscountAmount * 100 / totalMrp), 2) : 0;
                cartDetail.productLists = mappedData;

                // Prepare response
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = cartDetail;
                _response.Messages = "Cart products shown successfully.";

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

        #region GetProductCountInCart
        /// <summary>
        ///  Product count in cart.
        /// </summary>
        [HttpGet("GetProductCountInCart")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetProductCountInCart()
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
                var user = await _context.UserDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                var count = await _context.CartDetail.Where(u => u.UserDetailId == user.UserDetailId && u.ProductCountInCart > 0).CountAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = new { productCountInCart = count };
                _response.Messages = "Count retrieved successfully.";
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

        #region PlaceOrder
        /// <summary>
        ///  PlaceOrder for User.
        /// </summary>
        /// <returns></returns>
        [HttpPost("PlaceOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDTO model)
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
                var user = await _context.UserDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }
                if ((user.Address1 == null && user.Address2 == null) || userDetail.FirstName == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Please complete your profile to place an order.";
                    return Ok(_response);
                }

                var cart = await _context.CartDetail.Where(u => u.UserDetailId == user.UserDetailId).ToListAsync();
                if (cart.Count == 0)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Cart is empty.";
                    return Ok(_response);
                }

                var userOrder = new UserOrder()
                {
                    UserId = currentUserId,
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    PaymentMethod = model.PaymentMethod,
                    OrderDate = DateTime.UtcNow,
                };

                double? totalMrp = 0;
                double? totalDiscountAmount = 0;
                double? totalSellingPrice = 0;

                // foreach (var item in cart)
                // {
                //     var products = await _context.ProductStock
                //         .Where(u => u.ProductId == item.ProductId).ToListAsync();

                //     if (products.Count < item.ProductCountInCart)
                //     {
                //         _response.StatusCode = HttpStatusCode.OK;
                //         _response.IsSuccess = false;
                //         _response.Messages = ResponseMessages.msgNotFound + "record.";
                //         return Ok(_response);
                //     }

                //     for (int i = 0; i < item.ProductCountInCart; i++)
                //     {
                //         var cartItemToRemove = await _context.CartDetail
                //             .Where(u => u.ProductId == item.ProductId)
                //             .FirstOrDefaultAsync();

                //         var productStockToRemove = await _context.ProductStock
                //             .FirstOrDefaultAsync(u => u.ProductStockId == products[i].ProductStockId);

                //         _context.Remove(productStockToRemove);
                //         _context.Cart.Remove(cartItemToRemove);
                //     }

                //     await _context.SaveChangesAsync();
                // }

                foreach (var item in cart)
                {
                    var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);

                    totalMrp += product.TotalMrp * item.ProductCountInCart;
                    totalDiscountAmount += product.Discount * item.ProductCountInCart;
                    totalSellingPrice += product.SellingPrice * item.ProductCountInCart;
                }

                userOrder.TotalMrp = totalMrp;
                userOrder.TotalDiscountAmount = totalDiscountAmount;
                userOrder.TotalSellingPrice = totalSellingPrice;
                userOrder.TotalProducts = cart.Sum(u => u.ProductCountInCart);
                userOrder.OrderStatus = OrderStatus.Confirmed.ToString();
                userOrder.PaymentStatus = PaymentStatus.Unpaid.ToString();

                _context.Add(userOrder);
                await _context.SaveChangesAsync();

                foreach (var item in cart)
                {
                    var userOrderedProduct = new UserOrderedProduct();

                    userOrderedProduct.OrderId = userOrder.OrderId;

                    var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);
                    if (product == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    userOrderedProduct.ProductId = product.ProductId;
                    userOrderedProduct.SellingPricePerItem = product.SellingPrice;
                    userOrderedProduct.TotalMrp = product.TotalMrp;
                    userOrderedProduct.DiscountType = product.DiscountType;
                    userOrderedProduct.Discount = product.Discount;
                    userOrderedProduct.SellingPrice = (double)((double)product.SellingPrice * item.ProductCountInCart);
                    userOrderedProduct.ProductCount = item.ProductCountInCart;

                    _context.Add(userOrderedProduct);
                    await _context.SaveChangesAsync();
                }

                var response = _mapper.Map<OrderResponseDTO>(userOrder);
                response.CreateDate = userOrder.CreateDate.ToString(DefaultDateFormat);

                _context.RemoveRange(cart);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Order placed successfully.";
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

        #region CancelOrder
        /// <summary>
        ///  CancelOrder for User.
        /// </summary>
        /// <returns></returns>
        [HttpPost("CancelOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelOrder(CancelOrderDTO model)
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

                var orderDetail = await _context.UserOrder
                    .FirstOrDefaultAsync(u => u.OrderId == model.OrderId);

                if (orderDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                var orderedProducts = await _context.UserOrderedProduct
                    .Where(u => u.OrderId == model.OrderId)
                    .ToListAsync();

                foreach (var item in orderedProducts)
                {
                    item.ProductCount ??= 0;
                }
                await _context.SaveChangesAsync();

                orderDetail.OrderStatus = OrderStatus.Cancelled.ToString();
                orderDetail.CancelledBy = roleName;
                _context.Update(orderDetail);
                await _context.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Order cancelled successfully";
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

        #region GetAllProducts
        /// <summary>
        ///  Get All Products.
        /// </summary>
        [HttpGet]
        [Route("GetAllProducts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts(string? productType)
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

            if (string.IsNullOrEmpty(productType))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Product list shown successfully.";
                _response.Data = query.ToList();
                return Ok(_response);
            }
            else
            {
                if (productType == "Popular_Products")
                {
                    var popularProductIds = _context.OrderedPoduct
                                            .GroupBy(p => p.ProductId)
                                            .Select(g => g.Key)
                                            .ToList();

                    popularProductIds = popularProductIds == null ? _context.DistributorOrderedProduct
                           .GroupBy(p => p.ProductId)
                           .Select(g => g.Key)
                           .ToList() : popularProductIds;

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Product list shown successfully.";
                    _response.Data = query.ToList().Where(p => popularProductIds.Contains(p.ProductId))
                            .OrderByDescending(x => x.CreateDate)
                            .ToList();
                    return Ok(_response);

                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Product list shown successfully.";
                    _response.Data = query.ToList()
                            .OrderByDescending(x => x.CreateDate)
                            .ToList();

                    return Ok(_response);
                }

            }
        }
        #endregion


    }
}
