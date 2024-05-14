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
        public double TotalMrp { get; set; }
        public double Discount { get; set; }
        public int DiscountType { get; set; }
        public double SellingPrice { get; set; }

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
        public double TotalMrp { get; set; }
        public double Discount { get; set; }
        public int DiscountType { get; set; }
        public double SellingPrice { get; set; }
        //public bool IsDeleted { get; set; }
        public int RewardPoint { get; set; }
        public string CreateDate { get; set; }
    }

    public class ProductResponselistDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public string? MainCategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public bool? IsActive { get; set; }
        public double? TotalMrp { get; set; }
        public double? Discount { get; set; }
        public int? DiscountType { get; set; }
        public double? SellingPrice { get; set; }
        public double? DistributorDiscount { get; set; }
        public int? DistributorDiscountType { get; set; }
        public double? DistributorSellingPrice { get; set; }
        //public bool IsDeleted { get; set; }
        public int? RewardPoint { get; set; }
        public int? InStock { get; set; }
        public string CreateDate { get; set; }
    }

    public class ScannedProductlistDTO
    {
        public int DealerProductId { get; set; }
        public int DealerId { get; set; }
        public int DistributorId { get; set; }
        public int ProductId { get; set; }
        public int? RewardPoint { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string? Status { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public double? TotalMrp { get; set; }
        public double? Discount { get; set; }
        public double? SellingPrice { get; set; }
        public string? SerialNumber { get; set; }
        public string? ScannedDate { get; set; }
    }

    public class SetProductStatusDTO
    {
        public int productId { get; set; }
        public bool IsActive { get; set; }
    }

    public class RedeemProductRequestDTO
    {
        public int rewardProductId { get; set; }
        public int productCount { get; set; }
    }

    public class RedeemedProductDTO
    {
        public int ReedemProductId { get; set; }
        public int RewardProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int productCount { get; set; }
        public float MRP { get; set; }
        public int NeededPointToRedeem { get; set; }
        public string redeemedDate { get; set; }
        public string status { get; set; }
    }

    public class RedeemedProductStatusDTO
    {
        public int ReedemProductId { get; set; }
        public string status { get; set; }
    }


    public class ProductFiltrationListDTO
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int? mainProductCategoryId { get; set; }
        public string? mainCategoryName { get; set; }
        public int? subProductCategoryId { get; set; }
        public string? subCategoryName { get; set; }
        public int? brandId { get; set; }
        public string? searchQuery { get; set; }
    }

    public class AddProductQRAndSerialDTO
    {
        //public int ProductStockId { get; set; }
        public int ProductId { get; set; }
        public string SerialNumber { get; set; }

    }
    public class UpdateProductQRAndSerialDTO
    {
        public int ProductStockId { get; set; }
        public int ProductId { get; set; }
        public string SerialNumber { get; set; }
        public string? Status { get; set; }

    }
    public class ProductStockResponseDTO
    {
        public int productStockId { get; set; }
        public int productId { get; set; }
        public string serialNumber { get; set; }
        public string status { get; set; }
        public string createDate { get; set; }
        public string modifyDate { get; set; }

    }


    public class DashboardProductDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public string? MainCategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public bool? IsActive { get; set; }
        public double? TotalMrp { get; set; }
        public double? Discount { get; set; }
        public int? DiscountType { get; set; }
        public double? SellingPrice { get; set; }
        public double? DistributorDiscount { get; set; }
        public int? DistributorDiscountType { get; set; }
        public double? DistributorSellingPrice { get; set; }
        //public bool IsDeleted { get; set; }
        public int? RewardPoint { get; set; }
        public int? InStock { get; set; }
        public string CreateDate { get; set; }
    }

}
