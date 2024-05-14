namespace MaxemusAPI.Models.Dtos
{
    public class PlaceOrderRequestDTO
    {
        public int? CouponId { get; set; }
        public int? PaymentReceiptId { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class OrderResponseDTO
    {
        public long OrderId { get; set; }
        public string UserId { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public double? TotalMrp { get; set; }
        public double? TotalDiscountAmount { get; set; }
        public double? TotalSellingPrice { get; set; }
        public string OrderDate { get; set; }
        public string DeliveredDate { get; set; }
        public int? TotalProducts { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }
        public string CreateDate { get; set; }
    }
    public class DistributorOrderFiltrationListDTO
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string? paymentStatus { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string? orderStatus { get; set; }
        public string? searchQuery { get; set; }

    }

    public class OrderFiltrationListDTO
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int? distributorId { get; set; }
        public string? paymentStatus { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string? orderStatus { get; set; }
        public string? searchQuery { get; set; }
        public string? orderType { get; set; }
        public string? userId { get; set; }

    }
    public class ScannedFiltrationListDTO
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int? distributorId { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string? searchQuery { get; set; }
        public string? userId { get; set; }

    }
    public class DistributorOrderedListDTO
    {

        public long OrderId { get; set; }
        public string UserId { get; set; } = null!;
        public string ProductName { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public double? TotalMrp { get; set; }
        public double? TotalDiscountAmount { get; set; }
        public double? TotalSellingPrice { get; set; }
        public string OrderDate { get; set; }
        public string OrderTime { get; set; }
        public int? TotalProducts { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }

    }

    public class OrderedListDTO
    {
        public long OrderId { get; set; }
        public string UserId { get; set; } = null!;
        public string ProductName { get; set; }
        public string OrderProductInfo { get; set; }
        public int DistributorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public double? TotalMrp { get; set; }
        public double? TotalDiscountAmount { get; set; }
        public double? TotalSellingPrice { get; set; }
        public string OrderDate { get; set; }
        public DateTime OrderDatTime { get; set; }
        public string OrderTime { get; set; }
        public string OrderType { get; set; }
        public int? TotalProducts { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }

    }
    public class DistributorOrderedProductDTO
    {
        public int OrderedProductId { get; set; }
        public long OrderId { get; set; }
        public int? ProductId { get; set; }
        public double? SellingPricePerItem { get; set; }
        public double TotalMrp { get; set; }
        public int? DiscountType { get; set; }
        public double? Discount { get; set; }
        public double SellingPrice { get; set; }
        public int? ProductCount { get; set; }
        public double? ShippingCharges { get; set; }
    }


    public class DistributorOrderDetailDTO
    {
        public long OrderId { get; set; }
        public string UserId { get; set; }
        public string? TransactionId { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public double? TotalMrp { get; set; }
        public double? TotalDiscountAmount { get; set; }
        public double? TotalSellingPrice { get; set; }
        public string OrderDate { get; set; }
        public string DeliveredDate { get; set; }
        public int? TotalProducts { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }
        public List<DistributorOrderedProductDTO> DistributorOrderedProduct { get; set; }

    }
    public class CancelOrderDTO
    {
        public long OrderId { get; set; }

    }

    public class OrderStatusDTO
    {
        public long OrderId { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string? orderType { get; set; }

    }
}
