namespace MaxemusAPI.Models.Dtos
{
    public class CartDetailDTO
    {
        public int totalItem { get; set; }
        public double totalMrp { get; set; }
        public double totalDiscount { get; set; }
        public double totalDiscountAmount { get; set; }
        public double totalSellingPrice { get; set; }
        public List<ProductListFromCart> productLists { get; set; }
    }

    public class UserCartDetailDTO
    {
        public int totalItem { get; set; }
        public double totalMrp { get; set; }
        public double totalDiscount { get; set; }
        public double totalDiscountAmount { get; set; }
        public double totalSellingPrice { get; set; }
        public List<UserProductListFromCart> productLists { get; set; }
    }
    public class ProductListFromCart
    {
        public int ProductId { get; set; }
        public int? ProductCountInCart { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public double TotalMrp { get; set; }
        public double DistributorDiscount { get; set; }
        public int DistributorDiscountType { get; set; }
        public double DistributorSellingPrice { get; set; }
    }

    public class UserProductListFromCart
    {
        public int ProductId { get; set; }
        public int? ProductCountInCart { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public double TotalMrp { get; set; }
        public double Discount { get; set; }
        public int DiscountType { get; set; }
        public double SellingPrice { get; set; }
    }
}
