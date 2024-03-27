using MaxemusAPI.Common;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MaxemusAPI.Common.GlobalVariables;
using System.Net;

namespace MaxemusAPI.Repository.IRepository
{
    public interface IAdminRepository
    {
        Task<Object> UpdateProfile(AdminProfileRequestDTO model, string currentUserId);
        Task<Object> GetProfileDetail(string currentUserId);
        Task<Object> AddOrUpdateBrand(AddBrandDTO model);
        Task<Object> GetBrandDetail(int brandId);
        Task<Object> GetBrandList(NullableFilterationListDTO? model, string? CreatedBy);
        Task<Object> DeleteBrand(int brandId);
        Task<Object> AddCategory(AddCategoryDTO model);
        Task<Object> UpdateProductCategory(UpdateCategoryDTO model);
        Task<Object> GetCategoryList(GetCategoryRequestDTO model);
        Task<Object> DeleteCategory(DeleteCategoryDTO model);
        Task<Object> GetDistributorList(FilterationListDTO model);
        Task<Object> GetCategoryDetail(GetCategoryDetailRequestDTO model);
        Task<Object> SetDistributorStatus(SetDistributorStatusDTO model);
    }
}
