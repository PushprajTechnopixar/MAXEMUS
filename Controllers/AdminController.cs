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

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
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

        public AdminController(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
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
        ///  AddOrUpdateProfile for Distributor.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddOrUpdateCompanyProfile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddOrUpdateCompanyProfile([FromBody] AdminCompanyDTO model)
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

            if (model.CompanyId == 0)
            {
                var existingCompany = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (existingCompany == null)
                {
                    existingCompany = new CompanyDetail
                    {
                        UserId = currentUserId,
                        CompanyName = model.CompanyName,
                        RegistrationNumber = model.RegistrationNumber,
                        Image = model.Image,
                        CountryId = existingUser.CountryId,
                        StateId = existingUser.StateId,
                        City = model.City,
                        StreetAddress = model.StreetAddress,
                        Landmark = model.Landmark,
                        PostalCode = model.PostalCode,
                        PhoneNumber = model.PhoneNumber,
                        WhatsappNumber = model.WhatsAppNumber,
                        AboutUs = model.AboutUs
                    };

                    _context.Add(existingCompany);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Company added successfully.";
                    return Ok(_response);
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Company already exists.";
                return Ok(_response);
            }
            else if (model.CompanyId > 0)
            {
                var existingCompany = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.CompanyId == model.CompanyId && u.UserId == currentUserId);
                if (existingCompany != null)
                {
                    existingCompany.CompanyName = model.CompanyName;
                    existingCompany.RegistrationNumber = model.RegistrationNumber;
                    existingCompany.Image = model.Image;
                    existingCompany.CountryId = existingUser.CountryId;
                    existingCompany.StateId = existingUser.StateId;
                    existingCompany.City = model.City;
                    existingCompany.StreetAddress = model.StreetAddress;
                    existingCompany.Landmark = model.Landmark;
                    existingCompany.PostalCode = model.PostalCode;
                    existingCompany.PhoneNumber = model.PhoneNumber;
                    existingCompany.WhatsappNumber = model.WhatsAppNumber;
                    existingCompany.AboutUs = model.AboutUs;

                    _context.Update(existingCompany);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Company updated successfully.";
                    return Ok(_response);
                }
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = false;
            _response.Messages = "Invalid CompanyId.";
            return Ok(_response);

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
            var companyDetail = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (companyDetail == null)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgUserNotFound;
                return Ok(_response);
            }

            var mappedData = _mapper.Map<AdminResponseDTO>(userDetail);
            _mapper.Map(companyDetail, mappedData);

            var userCountryDetail = await _context.CountryMaster.Where(u => u.CountryId == mappedData.CountryId).FirstOrDefaultAsync();
            var userStateDetail = await _context.StateMaster.Where(u => u.StateId == mappedData.StateId).FirstOrDefaultAsync();
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserRequestDTO model)
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

            var mappedData = _mapper.Map(model, userDetail);
            _context.Update(userDetail);
            await _context.SaveChangesAsync();

            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(model, userProfileDetail);
            _context.ApplicationUsers.Update(updateProfile);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "Profile updated successfully.";
            return Ok(_response);
        }
        #endregion

        #region AddBrand
        /// <summary>
        /// Add brand.
        /// </summary>
        [HttpPost]
        [Route("AddBrand")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> AddBrand(AddBrandDTO model)
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

                var brand = _mapper.Map<Brand>(model);
                var isBrandExist = await _context.Brand.FirstOrDefaultAsync(u => u.BrandName.ToLower() == brand.BrandName.ToLower());
                if (isBrandExist != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = "Brand name already exists.";
                    return Ok(_response);
                }

                brand.BrandName = model.BrandName;
                _context.Brand.Add(brand);
                await _context.SaveChangesAsync();

                var getBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == brand.BrandId);

                if (getBrand != null)
                {
                    var brandDetail = _mapper.Map<BrandDTO>(getBrand);
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandDetail;
                    _response.Messages = "Brand" + ResponseMessages.msgAdditionSuccess;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgSomethingWentWrong;
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

        #region UpdateBrand
        /// <summary>
        /// Update brand.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("UpdateBrand")]
        [Authorize]
        public async Task<IActionResult> UpdateBrand(UpdateBrandDTO model)
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

                var isBrandExist = await _context.Brand.FirstOrDefaultAsync(u => (u.BrandName.ToLower() == model.BrandName.ToLower()
                && (u.BrandId != model.BrandId)));
                if (isBrandExist != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = "Brand name already exists.";
                    return Ok(_response);
                }

                var updteBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == model.BrandId);
                _mapper.Map(model, updteBrand);

                _context.Brand.Update(updteBrand);
                await _context.SaveChangesAsync();

                if (updteBrand != null)
                {
                    var brandDetail = _mapper.Map<BrandDTO>(updteBrand);
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandDetail;
                    _response.Messages = "Brand" + ResponseMessages.msgUpdationSuccess;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgSomethingWentWrong;
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
                var getBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == brandId);

                if (getBrand != null)
                {
                    var brandDetail = _mapper.Map<BrandDTO>(getBrand);
                    brandDetail.CreateDate = Convert.ToDateTime(brandDetail.CreateDate).ToString(@"dd-MM-yyyy");
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandDetail;
                    _response.Messages = "Brand detail" + ResponseMessages.msgShownSuccess;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgSomethingWentWrong;
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
            
                var brandList = _context.Brand.Select(item => new BrandListDTO
                {
                    BrandId = item.BrandId,
                    BrandName = item.BrandName,
                    CreateDate = item.CreateDate != null ? Convert.ToDateTime(item.CreateDate).ToString("dd-MM-yyyy") : null,
                    BrandImage = item.BrandImage
                }).OrderBy(u => u.BrandName).ToList();

                if (!string.IsNullOrEmpty(model.searchQuery))
                {
                    brandList = brandList.Where(u => u.BrandName.ToLower().StartsWith(model.searchQuery.ToLower())
                    ).ToList();
                }

                if (model.pageNumber > 0 && model.pageSize > 0)
                {
                    // Get's No of Rows Count   
                    int count = brandList.Count();

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int? CurrentPage = model.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int? PageSize = model.pageSize;

                    // Display TotalCount to Records to User  
                    int? TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int? TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Customer after applying Paging   
                    var items = brandList.Skip((int)((CurrentPage - 1) * PageSize)).Take((int)PageSize).ToList();

                    // if CurrentPage is greater than 1 means it has previousPage  
                    var previousPage = CurrentPage > 1 ? "Yes" : "No";

                    // if TotalPages is greater than CurrentPage means it has nextPage  
                    var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                    // Returing List of Customers Collections  
                    FilterationResponseModel<BrandListDTO> obj = new FilterationResponseModel<BrandListDTO>();
                    obj.totalCount = (int)TotalCount;
                    obj.pageSize = (int)PageSize;
                    obj.currentPage = (int)CurrentPage;
                    obj.totalPages = (int)TotalPages;
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
                    _response.Messages = "Brand list shown successfully.";
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandList;
                    _response.Messages = "Brand list shown successfully.";
                    return Ok(_response);
                }
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
                var getBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == brandId);

                if (getBrand != null)
                {
                    var getProduct = _context.Product.Where(u => u.BrandId == brandId).FirstOrDefault();
                    if (getProduct != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Can't delete, product is listed on this brand.";
                        return Ok(_response);
                    }
                    _context.Brand.Remove(getBrand);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Brand" + ResponseMessages.msgDeletionSuccess;
                    return Ok(_response);
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
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

                if (model.MainCategoryId == 0)
                {
                    var mainCategoryExists = await _context.MainCategory.AnyAsync(u => u.MainCategoryName.ToLower() == model.CategoryName.ToLower());
                    if (mainCategoryExists != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "mainCategory name already exists.";
                        return Ok(_response);
                    }

                    var newMainCategory = new MainCategory
                    {
                        MainCategoryName = model.CategoryName,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(newMainCategory);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "mainCategory added successfully.";
                    return Ok(_response);
                }

                if (model.MainCategoryId > 0)
                {
                    var mainCategory = await _context.MainCategory.FindAsync(model.MainCategoryId);
                    if (mainCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "mainCategory not found.";
                        return Ok(_response);
                    }

                    var subCategoryExists = await _context.SubCategory.AnyAsync(u => u.SubCategoryName.ToLower() == model.CategoryName.ToLower());
                    if (!subCategoryExists)
                    {
                        var newSubCategory = new SubCategory
                        {
                            MainCategoryId = model.MainCategoryId,
                            SubCategoryName = model.CategoryName,
                            CreateDate = DateTime.Now
                        };

                        _context.Add(newSubCategory);
                        await _context.SaveChangesAsync();

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "subCategory added successfully.";
                        return Ok(_response);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "subCategory name already exists.";
                    return Ok(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Invalid MainCategoryId.";
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

        #region UpdateCategory
        /// <summary>
        ///  Update category.
        /// </summary>
        [HttpPost("UpdateCategoryDTO")]
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


                if (model.MainCategoryId > 0)
                {
                    if (model.SubCategoryId > 0)
                    {
                        var subCategory = await _context.SubCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId && u.SubCategoryId == model.SubCategoryId);
                        if (subCategory != null)
                        {
                            subCategory.SubCategoryName = model.CategoryName;
                            subCategory.CreateDate = DateTime.Now;

                            _context.Update(subCategory);
                            await _context.SaveChangesAsync();

                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = true;
                            _response.Messages = "subCategory updated successfully.";
                            return Ok(_response);
                        }
                    }
                    else
                    {
                        var mainCategory = await _context.MainCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId);
                        if (mainCategory != null)
                        {
                            mainCategory.MainCategoryName = model.CategoryName;
                            mainCategory.CreateDate = DateTime.Now;

                            _context.Update(mainCategory);
                            await _context.SaveChangesAsync();

                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = true;
                            _response.Messages = "mainCategory updated successfully.";
                            return Ok(_response);
                        }
                    }
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
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

                if (model.MainCategoryId > 0)
                {
                    var subCategories = await _context.SubCategory.Where(u => u.MainCategoryId == model.MainCategoryId).ToListAsync();

                    if (subCategories.Count > 0)
                    {
                        var response = new List<CategoryDTO>();

                        foreach (var item in subCategories)
                        {
                            var categoryDTO = new CategoryDTO
                            {
                                MainCategoryId = item.MainCategoryId,
                                SubCategoryId = item.SubCategoryId,
                                CategoryName = item.SubCategoryName,
                                CategoryImage = item.SubCategoryImage,
                                CreateDate = item.CreateDate.ToString("dd-MM-yyyy")
                            };

                            response.Add(categoryDTO);
                        }


                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = response;
                        _response.Messages = "subcategories list shown successfully.";

                        return Ok(_response);
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "No subcategories found for the specified mainCategoryId.";
                    }

                    return Ok(_response);
                }
                else
                {
                    var mainCategories = await _context.MainCategory.ToListAsync();

                    var response = _mapper.Map<List<CategoryDTO>>(mainCategories);

                    foreach (var item in response)
                    {
                        var mainCategory = mainCategories.FirstOrDefault(u => u.MainCategoryId == item.MainCategoryId);
                        if (mainCategory != null)
                        {
                            item.CategoryName = mainCategory.MainCategoryName;
                            item.CategoryImage = mainCategory.MainCategoryImage;
                            item.CreateDate = mainCategory.CreateDate.ToString("dd-MM-yyyy");
                        }
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Category detail shown successfully.";

                    return Ok(_response);
                }
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

                CategoryDTO category = null;
                if (model.SubCategoryId > 0)
                {
                    var subCategoryDetail = await _context.SubCategory.FirstOrDefaultAsync(u => u.SubCategoryId == model.SubCategoryId);
                    if (subCategoryDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    category = _mapper.Map<CategoryDTO>(subCategoryDetail);

                    category.CategoryName = subCategoryDetail.SubCategoryName;
                    category.CategoryImage = subCategoryDetail.SubCategoryImage;
                    category.CreateDate = subCategoryDetail.CreateDate.ToString("dd-MM-yyyy");
                }
                else if (model.MainCategoryId > 0)
                {
                    var mainCategoryDetail = await _context.MainCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId);
                    if (mainCategoryDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    category = _mapper.Map<CategoryDTO>(mainCategoryDetail);

                    category.CategoryName = mainCategoryDetail.MainCategoryName;
                    category.CategoryImage = mainCategoryDetail.MainCategoryImage;
                    category.CreateDate = mainCategoryDetail.CreateDate.ToString("dd-MM-yyyy");
                }

                if (category != null)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = category;
                    _response.Messages = "category detail shown successfully.";
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "No category detail found.";
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

                if (model.SubCategoryId > 0 && model.MainCategoryId > 0)
                {
                    var subCategory = await _context.SubCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId && u.SubCategoryId == model.SubCategoryId);
                    if (subCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    _context.Remove(subCategory);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Subcategory deleted successfully.";
                    return Ok(_response);
                }
                else if (model.MainCategoryId > 0)
                {
                    var mainCategory = await _context.MainCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId);
                    if (mainCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return Ok(_response);
                    }

                    _context.Remove(mainCategory);

                    var subCategories = await _context.SubCategory.Where(u => u.MainCategoryId == model.MainCategoryId).ToListAsync();
                    if (subCategories == null || subCategories.Count == 0)
                    {
                        await _context.SaveChangesAsync();
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "MainCategory deleted successfully.";
                        return Ok(_response);
                    }

                    _context.RemoveRange(subCategories);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "MainCategory and its Subcategories deleted successfully.";
                    return Ok(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Invalid request.";
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
