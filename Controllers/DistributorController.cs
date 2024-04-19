﻿using AutoMapper;
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
        [Authorize(Roles = "Distributor,Admin")]
        public async Task<IActionResult> AddOrUpdateProfile([FromBody] DistributorRequestDTO model)
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
            var userProfileDetail = await _context.ApplicationUsers.FindAsync(currentUserId);
            if (userProfileDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "user.";
                return Ok(_response);
            }
            if (model.businessProfile.CountryId > 0)
            {
                var countryId = await _context.CountryMaster.FindAsync(model.businessProfile.CountryId);
                if (countryId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid countryId.";
                    return Ok(_response);
                }
            }
            if (model.businessProfile.StateId > 0)
            {
                var stateId = await _context.StateMaster.FindAsync(model.businessProfile.StateId);
                if (stateId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid stateId.";
                    return Ok(_response);
                }
            }
            if (model.personalProfile.countryId > 0)
            {
                var personalProfileCountryId = await _context.CountryMaster.FindAsync(model.personalProfile.countryId);
                if (model.personalProfile.countryId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid countryId.";
                    return Ok(_response);
                }
            }
            if (model.personalProfile.stateId > 0)
            {
                var personalProfileStateId = await _context.StateMaster.FindAsync(model.personalProfile.stateId);
                if (model.personalProfile.stateId == null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "please enter valid stateId.";
                    return Ok(_response);
                }
            }
            if (Gender.Male.ToString() != model.personalProfile.gender && Gender.Female.ToString() != model.personalProfile.gender && Gender.Others.ToString() != model.personalProfile.gender)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Please enter valid gender.";
                return Ok(_response);
            }

            var roles = await _userManager.GetRolesAsync(userDetail);
            var roleName = roles.FirstOrDefault();

            if (model.DistributorId > 0)
            {
                var distributorDetail = await _context.DistributorDetail
                    .FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId);
                if (distributorDetail == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail not found.";
                    return Ok(_response);
                }

                var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id.ToLower() == distributorDetail.UserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "Distributor detail not found.";
                    return Ok(_response);
                }

                if (user.Email != model.personalProfile.email)
                {
                    var ifUserNameUnique = _context.ApplicationUsers.Where(u => u.Id != distributorDetail.UserId && u.Email == model.personalProfile.email || u.PhoneNumber == model.personalProfile.phoneNumber).FirstOrDefault();
                    if (ifUserNameUnique != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Email or phone number already exists.";
                        return Ok(_response);
                    }
                    distributorDetail.Email = model.businessProfile.Email;
                    distributorDetail.Status = (distributorDetail.Status == Status.Incomplete.ToString() ? Status.Pending.ToString() : distributorDetail.Status);
                }

                _mapper.Map(model.personalProfile, user);
                _context.Update(user);
                await _context.SaveChangesAsync();

                if (distributorDetail.Email != model.businessProfile.Email)
                {
                    var ifUserNameUnique = _context.DistributorDetail.Where(u => u.UserId != distributorDetail.UserId && u.Email == model.businessProfile.Email).FirstOrDefault();
                    if (ifUserNameUnique != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Email or phone number already exists.";
                        return Ok(_response);
                    }
                    distributorDetail.Email = model.businessProfile.Email;
                    distributorDetail.Status = (distributorDetail.Status == Status.Incomplete.ToString() ? Status.Pending.ToString() : distributorDetail.Status);
                }

                var addressExists = await _context.DistributorAddress.FirstOrDefaultAsync(u => u.DistributorId == model.DistributorId && u.AddressType == AddressType.Individual.ToString());
                if (addressExists == null)
                {
                    addressExists = new DistributorAddress();
                    addressExists.DistributorId = model.DistributorId;
                    addressExists.AddressType = "Individual";
                    addressExists.Email = model.businessProfile.Email;
                    _mapper.Map(model.businessProfile, addressExists);
                    _context.Add(addressExists);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    addressExists.DistributorId = model.DistributorId;
                    addressExists.AddressType = "Individual";
                    addressExists.Email = model.businessProfile.Email;
                    _mapper.Map(model.businessProfile, addressExists);
                    _context.Update(addressExists);
                    await _context.SaveChangesAsync();
                }

                _mapper.Map(model.businessProfile, distributorDetail);
                _context.Update(distributorDetail);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorDetailsDTO>(user);

                response.personalProfile = _mapper.Map<UserResponseDTO>(user);
                response.businessProfile = _mapper.Map<DistributorBusinessResponseDTO>(distributorDetail);
                _mapper.Map(addressExists, response.businessProfile);

                response.personalProfile.AddressId = addressExists.AddressId;
                if (response.personalProfile.CountryId > 0)
                {
                    var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.personalProfile.CountryId).FirstOrDefaultAsync();
                    response.personalProfile.CountryName = distributorCountry.CountryName;
                }
                if (response.personalProfile.StateId > 0)
                {
                    var distributorState = await _context.StateMaster.Where(u => u.StateId == response.personalProfile.StateId).FirstOrDefaultAsync();
                    response.personalProfile.StateName = distributorState.StateName;
                }
                if (response.businessProfile.CountryId > 0)
                {
                    var businessCountry = await _context.CountryMaster.Where(u => u.CountryId == response.businessProfile.CountryId).FirstOrDefaultAsync();
                    response.businessProfile.CountryName = businessCountry.CountryName;
                }
                if (response.businessProfile.StateId > 0)
                {
                    var businessState = await _context.StateMaster.Where(u => u.StateId == response.businessProfile.StateId).FirstOrDefaultAsync();
                    response.businessProfile.StateName = businessState.StateName;
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = "Profile updated successfully.";
                _response.Data = response;
                return Ok(_response);
            }

            if (model.DistributorId == 0)
            {
                bool ifUserNameUnique = _userRepo.IsUniqueUser(model.personalProfile.email, model.personalProfile.phoneNumber);
                if (!ifUserNameUnique)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Email or phone number already exists.";
                    return Ok(_response);
                }

                ApplicationUser user = new()
                {
                    Email = model.personalProfile.email,
                    UserName = model.personalProfile.email,
                    NormalizedEmail = model.personalProfile.email.ToUpper(),
                    FirstName = model.personalProfile.firstName,
                    LastName = model.personalProfile.lastName,
                    PhoneNumber = model.personalProfile.phoneNumber,
                    CountryId = model.personalProfile.countryId,
                    StateId = model.personalProfile.stateId,
                    City = model.personalProfile.City,
                    PostalCode = model.personalProfile.PostalCode,
                    Gender = model.personalProfile.gender,
                    DialCode = model.personalProfile.dialCode,
                    DeviceType = model.personalProfile.deviceType
                };

                var result = await _userManager.CreateAsync(user, model.personalProfile.password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Distributor"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Distributor"));
                    }

                    await _userManager.AddToRoleAsync(user, "Distributor");

                }

                var distributorDetail = new DistributorDetail
                {
                    UserId = user.Id,
                    Email = model.businessProfile.Email
                };

                _mapper.Map(model.businessProfile, distributorDetail);
                _context.Add(distributorDetail);
                await _context.SaveChangesAsync();

                var distributorAddress = new DistributorAddress
                {
                    DistributorId = distributorDetail.DistributorId,
                    AddressType = "Individual",
                    Email = model.businessProfile.Email
                };
                _mapper.Map(model.businessProfile, distributorAddress);
                _context.Add(distributorAddress);
                await _context.SaveChangesAsync();

                distributorDetail.AddressId = distributorAddress.AddressId;
                distributorDetail.Status = Status.Pending.ToString();
                _context.Update(distributorDetail);
                await _context.SaveChangesAsync();

                var response = _mapper.Map<DistributorDetailsDTO>(user);
                response.personalProfile = _mapper.Map<UserResponseDTO>(user);
                response.businessProfile = _mapper.Map<DistributorBusinessResponseDTO>(distributorDetail);
                _mapper.Map(distributorAddress, response.businessProfile);

                response.UserId = user.Id;
                response.personalProfile.AddressId = distributorAddress.AddressId;

                if (response.personalProfile.CountryId > 0)
                {
                    var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.personalProfile.CountryId).FirstOrDefaultAsync();
                    response.personalProfile.CountryName = distributorCountry.CountryName;
                }
                if (response.personalProfile.StateId > 0)
                {
                    var distributorState = await _context.StateMaster.Where(u => u.StateId == response.personalProfile.StateId).FirstOrDefaultAsync();
                    response.personalProfile.StateName = distributorState.StateName;
                }
                if (response.businessProfile.CountryId > 0)
                {
                    var businessCountry = await _context.CountryMaster.Where(u => u.CountryId == response.businessProfile.CountryId).FirstOrDefaultAsync();
                    response.businessProfile.CountryName = businessCountry.CountryName;
                }
                if (response.businessProfile.StateId > 0)
                {
                    var businessState = await _context.StateMaster.Where(u => u.StateId == response.businessProfile.StateId).FirstOrDefaultAsync();
                    response.businessProfile.StateName = businessState.StateName;
                }

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
        public async Task<IActionResult> GetDistributorDetail(int? distributorId)
        {
            string currentUserId = HttpContext.User.Claims.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = "Token expired."
                });
            }

            var currentUserDetail = await _userManager.FindByIdAsync(currentUserId);
            if (currentUserDetail == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = ResponseMessages.msgUserNotFound
                });
            }

            DistributorDetail? distributorDetail = new DistributorDetail();
            distributorId = distributorId == null ? 0 : distributorId;

            if (distributorId > 0)
            {
                distributorDetail = await _context.DistributorDetail
                   .FirstOrDefaultAsync(u => u.DistributorId == distributorId);
            }
            else
            {
                distributorDetail = await _context.DistributorDetail
                   .FirstOrDefaultAsync(u => u.UserId == currentUserId);
            }

            if (distributorDetail == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = $"{ResponseMessages.msgNotFound} record."
                });
            }

            var distributor = await _userManager.FindByIdAsync(distributorDetail.UserId);
            if (distributor == null)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = false,
                    Messages = $"{ResponseMessages.msgNotFound} record."
                });
            }

            var response = new DistributorDetailsDTO
            {
                UserId = distributorDetail.UserId,
                personalProfile = _mapper.Map<UserResponseDTO>(distributor),
                businessProfile = _mapper.Map<DistributorBusinessResponseDTO>(distributorDetail)
            };

            response.businessProfile.DistributorId = distributorDetail.DistributorId;

            var distributorAddress = await _context.DistributorAddress
                .FirstOrDefaultAsync(u => u.DistributorId == distributorDetail.DistributorId);
            if (distributorAddress != null)
            {
                response.personalProfile.AddressId = distributorAddress.AddressId;
                _mapper.Map(distributorAddress, response.businessProfile);
            }

            if (response.personalProfile.CountryId > 0)
            {
                var distributorCountry = await _context.CountryMaster.Where(u => u.CountryId == response.personalProfile.CountryId).FirstOrDefaultAsync();
                response.personalProfile.CountryName = distributorCountry.CountryName;
            }
            if (response.personalProfile.StateId > 0)
            {
                var distributorState = await _context.StateMaster.Where(u => u.StateId == response.personalProfile.StateId).FirstOrDefaultAsync();
                response.personalProfile.StateName = distributorState.StateName;
            }
            if (response.businessProfile.CountryId > 0)
            {
                var businessCountry = await _context.CountryMaster.Where(u => u.CountryId == response.businessProfile.CountryId).FirstOrDefaultAsync();
                response.businessProfile.CountryName = businessCountry.CountryName;
            }
            if (response.businessProfile.StateId > 0)
            {
                var businessState = await _context.StateMaster.Where(u => u.StateId == response.businessProfile.StateId).FirstOrDefaultAsync();
                response.businessProfile.StateName = businessState.StateName;
            }

            return Ok(new
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Messages = "Distributor detail shown successfully.",
                Data = response,
            });
        }
        #endregion

        #region AddProductToCart
        /// <summary>
        ///  Add product to cart.
        /// </summary>
        [HttpPost("AddProductToCart")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Distributor")]
        public async Task<IActionResult> AddProductToCart([FromBody] int productId)
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

                var productStock = await _context.ProductStock.Where(u => u.ProductId == productId).ToListAsync();
                if (productStock.Count < 1)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.Messages = "product is out of stock.";
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
                        CreateDate = DateTime.UtcNow,
                    };
                    _context.Cart.Add(cart);
                }
                else
                {
                    cart.ProductCountInCart++;
                    _context.Cart.Update(cart);
                }

                await _context.SaveChangesAsync();

                var response = _mapper.Map<CartResponseDTO>(cart);
                _mapper.Map(product, response);
                response.CreateDate = cart.CreateDate?.ToString("dd-MM-yyyy");
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
        [Authorize(Roles = "Distributor")]
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
        [Authorize(Roles = "Distributor")]
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
        [Authorize(Roles = "Distributor")]
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
        [Authorize(Roles = "Distributor")]
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
                };

                double? totalMrp = 0;
                double? totalDiscountAmount = 0;
                double? totalSellingPrice = 0;

                foreach (var item in cart)
                {
                    var products = await _context.ProductStock
                        .Where(u => u.ProductId == item.ProductId).ToListAsync();

                    if (products.Count < item.ProductCountInCart)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    for (int i = 0; i < item.ProductCountInCart; i++)
                    {
                        var cartItemToRemove = await _context.Cart
                            .Where(u => u.ProductId == item.ProductId)
                            .FirstOrDefaultAsync();

                        var productStockToRemove = await _context.ProductStock
                            .FirstOrDefaultAsync(u => u.ProductStockId == products[i].ProductStockId);

                        _context.Remove(productStockToRemove);
                        _context.Cart.Remove(cartItemToRemove);
                    }

                    await _context.SaveChangesAsync();
                }



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

                    _context.Add(distributorOrderedProduct);
                    await _context.SaveChangesAsync();
                }



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
        [Authorize(Roles = "Distributor")]
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
        [Authorize(Roles = "Distributor")]
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

                response.CreateDate = distributorOrder.CreateDate.ToString(DefaultDateFormat);
                response.OrderDate = distributorOrder.OrderDate.ToString(DefaultDateFormat);
                response.DeliveredDate = distributorOrder.DeliveredDate?.ToString(DefaultDateFormat);

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
        [HttpPost("CancelOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "Distributor")]
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
            var distributorDetail = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.DistributorId == dealer.DistributorId);
            if (distributorDetail == null)
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
            _response.Messages = "Dealer status updated successfully.";

            return Ok(_response);
        }

        #endregion

    }
}
