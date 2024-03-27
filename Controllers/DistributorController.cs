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


        #region AddOrUpdateProfile
        /// <summary>
        ///  Add Or Update Profile for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddOrUpdateProfile")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddOrUpdateProfile([FromBody] DistributorDetailDTO model)
        {
            string currentUserId = (HttpContext.User.Claims.First().Value);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Token expired.";
                return Ok(_response);
            }

            if (AddressType.Individual.ToString() != model.DistributorAddress.AddressType
                && AddressType.Company.ToString() != model.DistributorAddress.AddressType
                && AddressType.Shipping.ToString() != model.DistributorAddress.AddressType
                && AddressType.Billing.ToString() != model.DistributorAddress.AddressType
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


            if (model.DistributorId > 0)
            {
                var distributorDetail = await _context.DistributorDetail
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId && u.DistributorId == model.DistributorId);
                if (distributorDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail not found.";
                    return Ok(_response);
                }

                var addressExists = await _context.DistributorAddress.FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId);
                if (addressExists == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Address not found.";
                    return Ok(_response);
                }

                addressExists.DistributorId = model.DistributorId;
                addressExists.AddressType = model.DistributorAddress.AddressType;
                addressExists.CountryId = model.DistributorAddress.CountryId;
                addressExists.StateId = model.DistributorAddress.StateId;
                addressExists.City = model.DistributorAddress.City;
                addressExists.HouseNoOrBuildingName = model.DistributorAddress.HouseNoOrBuildingName;
                addressExists.StreetAddress = model.DistributorAddress.StreetAddress;
                addressExists.Landmark = model.DistributorAddress.Landmark;
                addressExists.PostalCode = model.DistributorAddress.PostalCode;
                addressExists.PhoneNumber = model.DistributorAddress.PhoneNumber;


                _context.Update(addressExists);
                await _context.SaveChangesAsync();

                distributorDetail.Name = userProfileDetail.FirstName + " " + userProfileDetail.LastName;
                distributorDetail.RegistrationNumber = model.RegistrationNumber;
                distributorDetail.Description = model.Description;

                _context.Update(distributorDetail);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorDetailsDTO>(userProfileDetail);

                response.UserId = currentUserId;
                response.DistributorId = distributorDetail.DistributorId;
                response.AddressId = model.AddressId;
                response.Name = distributorDetail.Name;
                response.RegistrationNumber = distributorDetail.RegistrationNumber;
                response.Description = distributorDetail.Description;
                response.Image = distributorDetail.Image;
                response.Status = distributorDetail.Status;
                response.CreateDate = distributorDetail.CreateDate.ToShortDateString();
                response.ModifyDate = distributorDetail.ModifyDate.ToString();


                response.DistributorAddress = _mapper.Map<DistributorAddressDTO>(addressExists);

                var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.DistributorAddress.CountryId).FirstOrDefaultAsync();
                var distributorState = await _context.StateMaster.Where(u => u.StateId == response.DistributorAddress.StateId).FirstOrDefaultAsync();
                response.CountryName = distributorCountry.CountryName;
                response.StateName = distributorState.StateName;

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                _response.Data = response;
                return Ok(_response);
            }

            if (model.DistributorId == 0)
            {
                var distributorDetail = await _context.DistributorDetail
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (distributorDetail != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail already exists.";
                    return Ok(_response);
                }

                distributorDetail = new DistributorDetail
                {
                    UserId = currentUserId,
                    Name = userProfileDetail.FirstName + " " + userProfileDetail.LastName,
                    RegistrationNumber = model.RegistrationNumber,
                    Description = model.Description,
                };

                _context.Add(distributorDetail);
                await _context.SaveChangesAsync();

                var addressExists = await _context.DistributorAddress
                    .FirstOrDefaultAsync(u => u.DistributorId == distributorDetail.DistributorId);
                if (addressExists != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Address already exists for the distributor.";
                    return Ok(_response);
                }

                var distributorAddress = new DistributorAddress
                {
                    DistributorId = distributorDetail.DistributorId,
                    AddressType = model.DistributorAddress.AddressType,
                    CountryId = model.DistributorAddress.CountryId,
                    StateId = model.DistributorAddress.StateId,
                    City = model.DistributorAddress.City,
                    HouseNoOrBuildingName = model.DistributorAddress.HouseNoOrBuildingName,
                    StreetAddress = model.DistributorAddress.StreetAddress,
                    Landmark = model.DistributorAddress.Landmark,
                    PostalCode = model.DistributorAddress.PostalCode,
                    PhoneNumber = model.DistributorAddress.PhoneNumber
                };

                _context.Add(distributorAddress);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorDetailsDTO>(userProfileDetail);

                response.UserId = currentUserId;
                response.DistributorId = distributorDetail.DistributorId;
                response.AddressId = distributorAddress.AddressId;
                response.Name = distributorDetail.Name;
                response.RegistrationNumber = distributorDetail.RegistrationNumber;
                response.Description = distributorDetail.Description;
                response.Image = distributorDetail.Image;
                response.Status = distributorDetail.Status;
                response.CreateDate = distributorDetail.CreateDate.ToShortDateString();
                response.ModifyDate = distributorDetail.ModifyDate.ToString();


                response.DistributorAddress = _mapper.Map<DistributorAddressDTO>(distributorAddress);

                var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.DistributorAddress.CountryId).FirstOrDefaultAsync();
                var distributorState = await _context.StateMaster.Where(u => u.StateId == response.DistributorAddress.StateId).FirstOrDefaultAsync();
                response.CountryName = distributorCountry.CountryName;
                response.StateName = distributorState.StateName;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile added successfully.";
                _response.Data = response;
                return Ok(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Profile added successfully.";
            return Ok(_response);
        }

        #endregion

        #region GetDistributorDetail
        /// <summary>
        ///   Get Distributor Detail.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        [Route("GetDistributorDetail")]

        public async Task<IActionResult> GetDistributorDetail([FromQuery] string id)
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

            var distributor = _userManager.FindByIdAsync(id).GetAwaiter().GetResult();
            if (distributor == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.UserId == id);
            if (distributorDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }
            var distributorAddress = await _context.DistributorAddress.FirstOrDefaultAsync(u => u.DistributorId == distributorDetail.DistributorId);
            if (distributorAddress == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return Ok(_response);
            }


            var response = _mapper.Map<DistributorDetailsDTO>(distributor);

            response.UserId = id;
            response.DistributorId = distributorDetail.DistributorId;
            response.AddressId = distributorAddress.AddressId;
            response.Name = distributorDetail.Name;
            response.RegistrationNumber = distributorDetail.RegistrationNumber;
            response.Description = distributorDetail.Description;
            response.Image = distributorDetail.Image;
            response.Status = distributorDetail.Status;
            response.CreateDate = distributorDetail.CreateDate.ToShortDateString();
            response.ModifyDate = distributorDetail.ModifyDate.ToString();


            response.DistributorAddress = _mapper.Map<DistributorAddressDTO>(distributorAddress);

            var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.DistributorAddress.CountryId).FirstOrDefaultAsync();
            var distributorState = await _context.StateMaster.Where(u => u.StateId == response.DistributorAddress.StateId).FirstOrDefaultAsync();
            response.CountryName = distributorCountry.CountryName;
            response.StateName = distributorState.StateName;


            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "distributor detail shown successfully.";
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
        [Authorize]
        public async Task<IActionResult> AddProductToCart(int productId)
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

                var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == productId && u.IsActive == true);
                if (product == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Record not found.";
                    return Ok(_response);
                }

                var cart = await _context.Cart.FirstOrDefaultAsync(c => c.ProductId == productId && c.DistributorId == currentUserId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        ProductId = productId,
                        DistributorId = currentUserId,
                        ProductCountInCart = 1,
                        CreateDate = DateTime.Now
                    };
                    _context.Cart.Add(cart);
                }
                else
                {
                    cart.ModifyDate = DateTime.Now;
                    cart.ProductCountInCart++;
                    _context.Cart.Update(cart);
                }

                await _context.SaveChangesAsync();

                var response = _mapper.Map<CartResponseDTO>(cart);
                _mapper.Map(product, response);
                response.CreateDate = cart.CreateDate.ToString();
                response.TotalMrp = (double)(product.TotalMrp * cart.ProductCountInCart);
                response.Discount = (double)(product.Discount * cart.ProductCountInCart);
                response.DiscountType = product.DiscountType;

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
        [Authorize]
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

                var cart = await _context.Cart.Where(u => u.DistributorId == currentUserId).ToListAsync();
                if (cart.Count == 0)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                CartDetailDTO cartDetail = new CartDetailDTO();
                var mappedData = new List<ProductListFromCart>();

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
                        var productResponse = _mapper.Map<ProductListFromCart>(product);
                        productResponse.ProductCountInCart = item.ProductCountInCart;
                        productResponse.TotalMrp = (double)(product.TotalMrp * item.ProductCountInCart);
                        productResponse.Discount = (double)(product.Discount * item.ProductCountInCart);
                        productResponse.DiscountType = product.DiscountType;
                        productResponse.SellingPrice = productResponse.TotalMrp - productResponse.Discount;

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
                _response.Messages = "cart product list shown successfully.";

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

        #region RemoveProductFromCart
        /// <summary>
        ///  Remove product from cart.
        /// </summary>
        [HttpDelete("RemoveProductFromCart")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> RemoveProductFromCart(int productId)
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

                var cart = await _context.Cart.FirstOrDefaultAsync(u => u.DistributorId == currentUserId && u.ProductId == productId);

                if (cart == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Product not found.";
                    return Ok(_response);
                }
                else if (cart.ProductCountInCart > 0)
                {
                    cart.ProductCountInCart--;

                    if (cart.ProductCountInCart == 0)
                    {
                        _context.Cart.Remove(cart);
                    }

                    await _context.SaveChangesAsync();


                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Product removed successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Product count in cart is already zero.";
                    return Ok(_response);
                }


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
        [Authorize]
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

                var count = await _context.Cart.Where(u => u.DistributorId == currentUserId && u.ProductCountInCart > 0).CountAsync();

                if (count == 0)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = count.ToString();
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
        ///  PlaceOrder for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost("PlaceOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
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

                var cart = await _context.Cart.Where(u => u.DistributorId == currentUserId).ToListAsync();
                if (cart == null || !cart.Any())
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                var distributorOrder = new DistributorOrder()
                {
                    UserId = currentUserId,
                    FirstName = userDetail.FirstName,
                    LastName = userDetail.LastName,
                    PaymentMethod = model.PaymentMethod,
                    OrderDate = DateTime.UtcNow,
                    CreateDate = DateTime.UtcNow
                };

                double? totalMrp = 0;
                double? totalDiscountAmount = 0;
                double? totalSellingPrice = 0;

                foreach (var item in cart)
                {
                    var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);
                    if (product == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }


                    totalMrp += product.TotalMrp * item.ProductCountInCart;
                    totalDiscountAmount += product.Discount * item.ProductCountInCart;
                    totalSellingPrice += product.SellingPrice * item.ProductCountInCart;
                }

                distributorOrder.TotalMrp = totalMrp;
                distributorOrder.TotalDiscountAmount = totalDiscountAmount;
                distributorOrder.TotalSellingPrice = totalSellingPrice;
                distributorOrder.TotalProducts = cart.Sum(u => u.ProductCountInCart);

                _context.Add(distributorOrder);
                await _context.SaveChangesAsync();

                foreach (var item in cart)
                {
                    var distributorOrderedProduct = new DistributorOrderedProduct();

                    distributorOrderedProduct.OrderId = distributorOrder.OrderId;

                    var product = await _context.Product.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);
                    if (product == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    distributorOrderedProduct.ProductId = product.ProductId;
                    distributorOrderedProduct.SellingPricePerItem = product.SellingPrice;
                    distributorOrderedProduct.TotalMrp = product.TotalMrp;
                    distributorOrderedProduct.DiscountType = product.DiscountType;
                    distributorOrderedProduct.Discount = product.Discount;
                    distributorOrderedProduct.SellingPrice = product.SellingPrice;
                    distributorOrderedProduct.Quantity = item.ProductCountInCart;
                    distributorOrderedProduct.ProductCount = item.ProductCountInCart;
                    distributorOrderedProduct.CreateDate = DateTime.UtcNow;

                    _context.Add(distributorOrderedProduct);
                    await _context.SaveChangesAsync();
                }
                foreach (var item in cart)
                {
                    var productStock = new ProductStock();
                    var product = await _context.ProductStock.FirstOrDefaultAsync(u => u.ProductId == item.ProductId);
                    if (product == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }


                    _context.Remove(productStock);
                    await _context.SaveChangesAsync();

                }
                _context.RemoveRange(cart);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<OrderResponseDTO>(distributorOrder);
                response.CreateDate = distributorOrder.CreateDate.ToShortDateString();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "order placed successfully.";
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

        #region OrderList
        /// <summary>
        ///  Get OrderList for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpGet("OrderList")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> OrderList([FromQuery] DistributorOrderFiltrationListDTO model)
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

                List<DistributorOrder> orderList;
                orderList = (await _context.DistributorOrder.Where(u => u.UserId == currentUserId).ToListAsync()).OrderByDescending(u => u.OrderDate).ToList();

                if (model.fromDate != null && model.toDate != null)
                {
                    orderList = orderList.Where(x => (x.OrderDate.Date >= model.fromDate) && (x.OrderDate.Date <= model.toDate)).ToList();
                }

                var orderIds = orderList.Select(order => order.OrderId).ToList();
                var productId = await _context.DistributorOrderedProduct.Where(u => orderIds.Contains(u.OrderId)).ToListAsync();

                var productName = await _context.Product.ToListAsync();
                var response = _mapper.Map<List<DistributorOrderedListDTO>>(orderList);

                foreach (var item in response)
                {
                    var ctz = TZConvert.GetTimeZoneInfo("India Standard Time");
                    var convertedZoneDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.OrderDate), ctz);
                    item.OrderTime = Convert.ToDateTime(convertedZoneDate).ToString(@"hh:mm tt");
                    item.OrderDate = Convert.ToDateTime(convertedZoneDate).ToString(@"dd-MM-yyyy");
                    item.CreateDate = Convert.ToDateTime(convertedZoneDate).ToString(@"dd-MM-yyyy");

                }

                if (!string.IsNullOrEmpty(model.paymentStatus))
                {
                    response = response.Where(x => (x.PaymentStatus == model.paymentStatus)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(model.orderStatus))
                {
                    response = response.Where(x => (x.OrderStatus == model.orderStatus)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(model.searchQuery))
                {
                    response = response.Where(x => (x.PaymentStatus?.IndexOf(model.searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }



                // Get's No of Rows Count   
                int count = response.Count();

                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = model.pageNumber;

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = model.pageSize;

                // Display TotalCount to Records to User  
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging   
                var items = response.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Returing List of Customers Collections  
                FilterationResponseModel<DistributorOrderedListDTO> obj = new FilterationResponseModel<DistributorOrderedListDTO>();
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
        ///  Get OrderDetail for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpGet("OrderDetail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> OrderDetail(long orderId)
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

                var response = _mapper.Map<DistributorOrderDetailDTO>(distributorOrder);
                response.DistributorOrderedProduct = _mapper.Map<List<DistributorOrderedProductDTO>>(distributorOrderProducts);

                response.CreateDate = distributorOrder.CreateDate.ToShortDateString();
                response.OrderDate = distributorOrder.OrderDate.ToShortDateString();
                response.DeliveredDate = distributorOrder.DeliveredDate.ToString();

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "order detail shown successfully.";
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
        ///  CancelOrder for Distributor.
        /// </summary>
        /// <returns></returns>
        ///    [HttpPost]
        [HttpPost("CancelOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
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

                var orderDetail = await _context.DistributorOrder
                    .FirstOrDefaultAsync(u => u.OrderId == model.OrderId);

                if (orderDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return Ok(_response);
                }

                var orderedProducts = await _context.DistributorOrderedProduct
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

    }
}
