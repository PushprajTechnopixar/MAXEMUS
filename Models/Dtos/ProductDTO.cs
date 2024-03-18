using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class AddProductDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

    }

    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        [Required] public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int RewardPoint { get; set; }
        public string CreateDate { get; set; }
    }

    public class SetProductStatusDTO
    {
        public int productId { get; set; }
        public int status { get; set; }
    }

    public class ProductFiltrationListDTO
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int? mainProductCategoryId { get; set; }
        public int? subProductCategoryId { get; set; }
        public int? brandId { get; set; }
        public string? searchQuery { get; set; }
    }
}
