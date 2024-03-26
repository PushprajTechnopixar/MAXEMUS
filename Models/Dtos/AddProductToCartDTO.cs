using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class AddProductToCartDTO
    {
        public int CartId { get; set; }
        public int? ProductId { get; set; }
        public string? DistributorId { get; set; }
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
        public double Discount { get; set; }
        public int DiscountType { get; set; }
        public double SellingPrice { get; set; }
        public string? CreateDate { get; set; }

    }
}
