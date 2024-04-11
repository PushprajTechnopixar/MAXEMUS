using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models
{
    public partial class AddCategoryDTO
    {
        public int MainCategoryId { get; set; }
        [Required] public string CategoryName { get; set; }
        public string Description { get; set; }
    }
    public partial class UpdateCategoryDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

    }
    public partial class CategoryDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string CategoryImage { get; set; }
        public bool isNext { get; set; } = false;
        public string CreateDate { get; set; }
    }
    public class CategoryResponseDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string CreateDate { get; set; }
    }
    public partial class CategoryImageDTO
    {
        public int? MainCategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string? CategoryImage { get; set; }

    }
    public partial class CategoryRequestDTO
    {
        public int? MainCategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string? MainCategoryName { get; set; }
        public string? SubCategoryName { get; set; }
    }

    public partial class GetCategoryRequestDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }
    public partial class GetCategoryDetailRequestDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }

    public partial class DeleteCategoryDTO
    {
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
    }
}
