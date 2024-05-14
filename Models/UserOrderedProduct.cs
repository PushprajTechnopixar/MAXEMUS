using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class UserOrderedProduct
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
        public DateTime CreateDate { get; set; }

        public virtual UserOrder Order { get; set; } = null!;
        public virtual Product? Product { get; set; }
    }
}
