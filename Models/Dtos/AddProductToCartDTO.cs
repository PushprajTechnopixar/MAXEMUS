using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class AddProductToCartDTO
    {
        public int ProductId { get; set; }
        public int? ProductCountInCart { get; set; }
    }

    public class CartResponseDTO
    {
        public int CartId { get; set; }
        public int? ProductId { get; set; }
        public string? DistributorId { get; set; }
        public int? ProductCountInCart { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public double TotalMrp { get; set; }
        public double DistributorDiscount { get; set; }
        public int DistributorDiscountType { get; set; }
        public double DistributorSellingPrice { get; set; }
        public string? CreateDate { get; set; }

    }
}
