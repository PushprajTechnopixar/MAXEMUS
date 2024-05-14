using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class UserOrder
    {
        public UserOrder()
        {
            UserOrderedProduct = new HashSet<UserOrderedProduct>();
        }

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
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public int? TotalProducts { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ICollection<UserOrderedProduct> UserOrderedProduct { get; set; }
    }
}
