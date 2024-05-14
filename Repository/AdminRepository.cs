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
using GSF.Console;

namespace MaxemusAPI.Repository
{
    public class AdminRepository : IAdminRepository
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

        public AdminRepository(IAccountRepository userRepo, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context, IConfiguration configuration,
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


        public async Task<Object> UpdateProfile(AdminProfileRequestDTO model, string currentUserId)
        {
            var userDetail = await _context.ApplicationUsers.FindAsync(currentUserId);

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
                        return _response;
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
                        return _response;
                    }
                }
            }

            var mappedData = _mapper.Map(model, userDetail);
            _context.Update(userDetail);
            await _context.SaveChangesAsync();

            var userProfileDetail = await _context.ApplicationUsers.Where(u => u.Id == currentUserId).FirstOrDefaultAsync();
            var updateProfile = _mapper.Map(model, userProfileDetail);
            _context.ApplicationUsers.Update(updateProfile);
            await _context.SaveChangesAsync();

            var existingCompany = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (existingCompany == null)
            {
                existingCompany = new CompanyDetail
                {
                    UserId = currentUserId,
                    CompanyName = model.companyProfile.companyName,
                    RegistrationNumber = model.companyProfile.registrationNumber,
                    CountryId = model.companyProfile.countryId,
                    StateId = model.companyProfile.stateId,
                    City = model.City,
                    StreetAddress = model.companyProfile.streetAddress,
                    BuildingNameOrNumber = model.companyProfile.BuildingNameOrNumber,
                    Landmark = model.companyProfile.landmark,
                    PostalCode = model.companyProfile.postalCode,
                    PhoneNumber = model.companyProfile.phoneNumber,
                    WhatsappNumber = model.companyProfile.whatsAppNumber,
                    AboutUs = model.companyProfile.aboutUs,
                    Timing = model.companyProfile.Timing,
                    TwitterLink = model.companyProfile.TwitterLink,
                    FacebookLink = model.companyProfile.facebookLink,
                    InstagramLink = model.companyProfile.InstagramLink,
                };

                _context.Add(existingCompany);
                await _context.SaveChangesAsync();
            }
            else
            {
                existingCompany.CompanyName = model.companyProfile.companyName;
                existingCompany.RegistrationNumber = model.companyProfile.registrationNumber;
                existingCompany.CountryId = model.companyProfile.countryId;
                existingCompany.StateId = model.companyProfile.stateId;
                existingCompany.City = model.City;
                existingCompany.StreetAddress = model.companyProfile.streetAddress;
                existingCompany.Landmark = model.companyProfile.landmark;
                existingCompany.PostalCode = model.companyProfile.postalCode;
                existingCompany.PhoneNumber = model.companyProfile.phoneNumber;
                existingCompany.WhatsappNumber = model.companyProfile.whatsAppNumber;
                existingCompany.AboutUs = model.companyProfile.aboutUs;

                _context.Update(existingCompany);
                await _context.SaveChangesAsync();
            }

            //var companyResponse = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);
            var response = _mapper.Map<AdminResponseDTO>(userProfileDetail);
            response.companyProfile = _mapper.Map<AdminCompanyResponseDTO>(existingCompany);
            response.userId = userDetail.Id;
            if (response.countryId > 0)
            {
                var userCountry = await _context.CountryMaster.Where(u => u.CountryId == response.countryId).FirstOrDefaultAsync();
                response.countryName = userCountry.CountryName;
            }
            if (response.stateId > 0)
            {
                var userState = await _context.StateMaster.Where(u => u.StateId == response.stateId).FirstOrDefaultAsync();
                response.stateName = userState.StateName;
            }
            if (response.companyProfile.CountryId > 0)
            {
                var companyCountry = await _context.CountryMaster.Where(u => u.CountryId == response.companyProfile.CountryId).FirstOrDefaultAsync();
                response.companyProfile.countryName = companyCountry.CountryName;
            }
            if (response.companyProfile.StateId > 0)
            {
                var companyState = await _context.StateMaster.Where(u => u.StateId == response.companyProfile.StateId).FirstOrDefaultAsync();
                response.companyProfile.stateName = companyState.StateName;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Company updated successfully.";
            return _response;
        }

        public async Task<Object> GetProfileDetail(string currentUserId)
        {
            var userDetail = _userManager.FindByIdAsync(currentUserId).GetAwaiter().GetResult();

            var companyDetail = await _context.CompanyDetail.FirstOrDefaultAsync(u => u.UserId == currentUserId);

            var response = _mapper.Map<AdminResponseDTO>(userDetail);
            if (companyDetail != null)
            {
                response.companyProfile = _mapper.Map<AdminCompanyResponseDTO>(companyDetail);
                response.userId = userDetail.Id;
                if (response.countryId > 0)
                {
                    var userCountry = await _context.CountryMaster.Where(u => u.CountryId == response.countryId).FirstOrDefaultAsync();
                    response.countryName = userCountry.CountryName;
                }
                if (response.stateId > 0)
                {
                    var userState = await _context.StateMaster.Where(u => u.StateId == response.stateId).FirstOrDefaultAsync();
                    response.stateName = userState.StateName;
                }
                if (response.companyProfile.CountryId > 0)
                {
                    var companyCountry = await _context.CountryMaster.Where(u => u.CountryId == response.companyProfile.CountryId).FirstOrDefaultAsync();
                    response.companyProfile.countryName = companyCountry.CountryName;
                }
                if (response.companyProfile.StateId > 0)
                {
                    var companyState = await _context.StateMaster.Where(u => u.StateId == response.companyProfile.StateId).FirstOrDefaultAsync();
                    response.companyProfile.stateName = companyState.StateName;
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Data = response;
                _response.Messages = "Detail" + ResponseMessages.msgShownSuccess;
                return _response;
            }

            response.userId = userDetail.Id;

            if (response.countryId > 0)
            {
                var userCountry = await _context.CountryMaster.Where(u => u.CountryId == response.countryId).FirstOrDefaultAsync();
                response.countryName = userCountry.CountryName;
            }
            if (response.stateId > 0)
            {
                var userState = await _context.StateMaster.Where(u => u.StateId == response.stateId).FirstOrDefaultAsync();
                response.stateName = userState.StateName;
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = response;
            _response.Messages = "Detail" + ResponseMessages.msgShownSuccess;
            return _response;
        }

        public async Task<Object> AddOrUpdateBrand(AddBrandDTO model)
        {
            try
            {
                var brand = _mapper.Map<Brand>(model);
                if (model.BrandId == 0)
                {
                    var isBrandExist = await _context.Brand.FirstOrDefaultAsync(u => u.BrandName.ToLower() == brand.BrandName.ToLower());
                    if (isBrandExist != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Data = new Object { };
                        _response.Messages = "Brand name already exists.";
                        return _response;
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
                        return _response;
                    }
                }
                else
                {
                    var updateBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == model.BrandId);
                    _mapper.Map(model, updateBrand);

                    _context.Brand.Update(updateBrand);
                    await _context.SaveChangesAsync();

                    if (updateBrand != null)
                    {
                        var brandDetail = _mapper.Map<BrandDTO>(updateBrand);
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = brandDetail;
                        _response.Messages = "Brand" + ResponseMessages.msgUpdationSuccess;
                        return _response;
                    }
                }
                return _response;
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return _response;
            }
        }

        public async Task<Object> GetBrandDetail(int brandId)
        {
            try
            {
                var getBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == brandId);

                if (getBrand != null)
                {
                    var brandDetail = _mapper.Map<BrandDTO>(getBrand);
                    brandDetail.CreateDate = Convert.ToDateTime(brandDetail.CreateDate).ToString(@"dd-MM-yyyy");
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandDetail;
                    _response.Messages = "Brand detail" + ResponseMessages.msgShownSuccess;
                    return _response;
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgNotFound;
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return _response;
            }
        }

        public async Task<Object> GetBrandList(NullableFilterationListDTO? model, string? CreatedBy)
        {
            try
            {
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
                        return _response;
                    }
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = obj;
                    _response.Messages = "Brand list shown successfully.";
                    return _response;
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = brandList;
                    _response.Messages = "Brand list shown successfully.";
                    return _response;
                }
            }
            catch (System.Exception ex)
            {

                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return _response;
            }
        }

        public async Task<Object> DeleteBrand(int brandId)
        {
            try
            {
                var getBrand = await _context.Brand.FirstOrDefaultAsync(u => u.BrandId == brandId);

                if (getBrand != null)
                {
                    var getProduct = _context.Product.Where(u => u.BrandId == brandId).FirstOrDefault();
                    if (getProduct != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Can't delete, product is listed on this brand.";
                        return _response;
                    }
                    _context.Brand.Remove(getBrand);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Brand" + ResponseMessages.msgDeletionSuccess;
                    return _response;
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Data = new Object { };
                    _response.Messages = ResponseMessages.msgNotFound + "record.";
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Data = new { };
                _response.Messages = ResponseMessages.msgSomethingWentWrong + ex.Message;
                return _response;
            }
        }

        public async Task<Object> AddCategory(AddCategoryDTO model)
        {
            try
            {
                if (model.MainCategoryId == 0)
                {
                    var mainCategoryExists = await _context.MainCategory.Where(u => u.MainCategoryName.ToLower() == model.CategoryName.ToLower()).FirstOrDefaultAsync();
                    if (mainCategoryExists != null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Category name already exists.";
                        return _response;
                    }

                    var newMainCategory = new MainCategory
                    {
                        MainCategoryName = model.CategoryName,
                        Description = model.Description,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(newMainCategory);
                    await _context.SaveChangesAsync();

                    var response = _mapper.Map<CategoryResponseDTO>(newMainCategory);
                    response.CategoryName = model.CategoryName;
                    response.CreateDate = newMainCategory.CreateDate.ToShortDateString();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "Category added successfully.";
                    _response.Data = response;
                    return _response;



                }

                if (model.MainCategoryId > 0)
                {
                    var mainCategory = await _context.MainCategory.FindAsync(model.MainCategoryId);
                    if (mainCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "Category not found.";
                        return _response;
                    }

                    var subCategoryExists = await _context.SubCategory.AnyAsync(u => u.SubCategoryName.ToLower() == model.CategoryName.ToLower());
                    if (!subCategoryExists)
                    {
                        var newSubCategory = new SubCategory
                        {
                            MainCategoryId = model.MainCategoryId,
                            SubCategoryName = model.CategoryName,
                            Description = model.Description,
                            CreateDate = DateTime.Now
                        };

                        _context.Add(newSubCategory);
                        await _context.SaveChangesAsync();

                        var response = _mapper.Map<CategoryResponseDTO>(newSubCategory);
                        response.CategoryName = model.CategoryName;
                        response.CreateDate = newSubCategory.CreateDate.ToShortDateString();

                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "Category added successfully.";
                        _response.Data = response;
                        return _response;
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = false;
                    _response.Messages = "Category name already exists.";
                    return _response;
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Invalid MainCategoryId.";
                return _response;

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return _response;
            }
        }

        public async Task<Object> UpdateProductCategory(UpdateCategoryDTO model)
        {
            try
            {
                if (model.MainCategoryId > 0)
                {
                    if (model.SubCategoryId > 0)
                    {
                        var subCategory = await _context.SubCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId && u.SubCategoryId == model.SubCategoryId);
                        if (subCategory != null)
                        {
                            subCategory.SubCategoryName = model.CategoryName;
                            subCategory.Description = model.Description;
                            subCategory.ModifyDate = DateTime.Now;

                            _context.Update(subCategory);
                            await _context.SaveChangesAsync();

                            var response = _mapper.Map<CategoryResponseDTO>(subCategory);
                            response.CategoryName = model.CategoryName;
                            response.CreateDate = subCategory.ModifyDate.ToShortDateString();

                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = true;
                            _response.Messages = "Category updated successfully.";
                            _response.Data = response;
                            return _response;
                        }
                    }
                    else
                    {
                        var mainCategory = await _context.MainCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId);
                        if (mainCategory != null)
                        {
                            mainCategory.MainCategoryName = model.CategoryName;
                            mainCategory.Description = model.Description;
                            mainCategory.ModifyDate = DateTime.Now;

                            _context.Update(mainCategory);
                            await _context.SaveChangesAsync();

                            var response = _mapper.Map<CategoryResponseDTO>(mainCategory);
                            response.CategoryName = model.CategoryName;
                            response.CreateDate = mainCategory.ModifyDate.ToShortDateString();

                            _response.StatusCode = HttpStatusCode.OK;
                            _response.IsSuccess = true;
                            _response.Messages = "Category updated successfully.";
                            _response.Data = response;
                            return _response;
                        }
                    }
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgNotFound + "record.";
                return _response;

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return _response;
            }
        }

        public async Task<Object> GetCategoryList(GetCategoryRequestDTO model)
        {
            try
            {
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
                                Description = item.Description,
                                CreateDate = item.CreateDate.ToString("dd-MM-yyyy")
                            };

                            response.Add(categoryDTO);
                        }


                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Data = response;
                        _response.Messages = "category list shown successfully.";

                        return _response;
                    }
                    else
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = "No categories found for the specified CategoryId.";
                    }

                    return _response;
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
                            item.Description = mainCategory.Description;
                            item.CreateDate = mainCategory.CreateDate.ToString("dd-MM-yyyy");
                        }
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Data = response;
                    _response.Messages = "Category list shown successfully.";

                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return _response;
            }
        }

        public async Task<Object> DeleteCategory(DeleteCategoryDTO model)
        {
            try
            {
                if (model.SubCategoryId > 0 && model.MainCategoryId > 0)
                {
                    var subCategory = await _context.SubCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId && u.SubCategoryId == model.SubCategoryId);
                    if (subCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return _response;
                    }

                    _context.Remove(subCategory);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "category deleted successfully.";
                    return _response;
                }
                else if (model.MainCategoryId > 0)
                {
                    var mainCategory = await _context.MainCategory.FirstOrDefaultAsync(u => u.MainCategoryId == model.MainCategoryId);
                    if (mainCategory == null)
                    {
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return _response;
                    }

                    _context.Remove(mainCategory);

                    var subCategories = await _context.SubCategory.Where(u => u.MainCategoryId == model.MainCategoryId).ToListAsync();
                    if (subCategories == null || subCategories.Count == 0)
                    {
                        await _context.SaveChangesAsync();
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        _response.Messages = "Category deleted successfully.";
                        return _response;
                    }

                    _context.RemoveRange(subCategories);
                    await _context.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Messages = "categories deleted successfully.";
                    return _response;
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = false;
                _response.Messages = "Invalid request.";
                return _response;

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return _response;
            }
        }

        public async Task<Object> GetDistributorList(FilterationListDTO model)
        {
            var distributorUser = await _context.DistributorDetail.Where(u => u.Status != Status.Incomplete.ToString()).OrderByDescending(u => u.CreateDate).ToListAsync();

            List<DistributorUserListDTO> distributorUserList = new List<DistributorUserListDTO>();

            foreach (var item in distributorUser)
            {
                var distributorUserProfileDetail = await _context.ApplicationUsers
                    .Where(u => u.Id == item.UserId && u.IsDeleted == false)
                    .FirstOrDefaultAsync();

                var distributorAddress = await _context.DistributorAddress
                .Where(u => u.DistributorId == item.DistributorId && u.AddressType == AddressType.Individual.ToString())
                .FirstOrDefaultAsync();

                if (distributorUserProfileDetail != null)
                {
                    var mappedData = _mapper.Map<DistributorUserListDTO>(item);
                    mappedData.distributorId = item.DistributorId;
                    mappedData.userId = distributorUserProfileDetail.Id;
                    mappedData.companyName = item.Name;
                    mappedData.email = distributorUserProfileDetail.Email;
                    mappedData.firstName = distributorUserProfileDetail.FirstName;
                    mappedData.lastName = distributorUserProfileDetail.LastName;
                    mappedData.profilePic = distributorUserProfileDetail.ProfilePic;
                    mappedData.gender = distributorUserProfileDetail.Gender;
                    mappedData.Status = item.Status;
                    mappedData.createDate = item.CreateDate.ToShortDateString();
                    if (distributorAddress != null)
                    {
                        mappedData.phoneNumber = distributorAddress.PhoneNumber;
                        mappedData.streetAddress = distributorAddress?.StreetAddress;
                    }

                    distributorUserList.Add(mappedData);
                }
            }

            if (!string.IsNullOrEmpty(model.searchQuery))
            {
                distributorUserList = distributorUserList.Where(u => u.firstName.ToLower().Contains(model.searchQuery.ToLower())
                || u.email.ToLower().Contains(model.searchQuery.ToLower())
                || u.companyName.ToLower().Contains(model.searchQuery.ToLower())
                ).ToList();
            }

            // Get's No of Rows Count   
            int count = distributorUserList.Count();

            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            int CurrentPage = model.pageNumber;

            // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
            int PageSize = model.pageSize;

            // Display TotalCount to Records to User  
            int TotalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            // Returns List of Customer after applying Paging   
            var items = distributorUserList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            // if CurrentPage is greater than 1 means it has previousPage  
            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            // if TotalPages is greater than CurrentPage means it has nextPage  
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            // Returing List of Customers Collections  
            FilterationResponseModel<DistributorUserListDTO> obj = new FilterationResponseModel<DistributorUserListDTO>();
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
                return _response;
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Data = obj;
            _response.Messages = "List shown successfully.";
            return _response;

        }

        public async Task<Object> GetCategoryDetail(GetCategoryDetailRequestDTO model)
        {
            try
            {
                CategoryDTO category = null;
                if (model.SubCategoryId > 0)
                {
                    var subCategoryDetail = await _context.SubCategory.FirstOrDefaultAsync(u => u.SubCategoryId == model.SubCategoryId);
                    if (subCategoryDetail == null)
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.IsSuccess = false;
                        _response.Messages = ResponseMessages.msgNotFound + "record.";
                        return _response;
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
                        return _response;
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

                return _response;

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ex.Message;
                return _response;
            }
        }

        public async Task<Object> SetDistributorStatus(SetDistributorStatusDTO model)
        {

            var distributor = await _context.DistributorDetail.FirstOrDefaultAsync(u => u.DistributorId == model.distributorId);

            distributor.Status = model.status.ToString();

            _context.Update(distributor);
            await _context.SaveChangesAsync();

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Messages = "distributor status updated successfully.";

            return _response;
        }

    }
}

