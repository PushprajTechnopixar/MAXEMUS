using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class OrderedPoduct
    {
        public int OrderedProductId { get; set; }
        public int OrderDetailId { get; set; }
        public int? ProductId { get; set; }
        public double TotalMrp { get; set; }
        public int? DiscountType { get; set; }
        public double? Discount { get; set; }
        public double SellingPrice { get; set; }
        public double? Quantity { get; set; }
        public int? ProductCount { get; set; }
        public double? ShippingCharges { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
