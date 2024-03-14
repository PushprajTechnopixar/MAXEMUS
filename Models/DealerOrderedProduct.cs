﻿using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class DealerOrderedProduct
    {
        public int OrderedProductId { get; set; }
        public long OrderId { get; set; }
        public int? ProductId { get; set; }
        public double? SellingPricePerItem { get; set; }
        public double TotalMrp { get; set; }
        public int? DiscountType { get; set; }
        public double? Discount { get; set; }
        public double SellingPrice { get; set; }
        public double? Quantity { get; set; }
        public int? ProductCount { get; set; }
        public double? ShippingCharges { get; set; }
        public int? RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual DealerOrder Order { get; set; } = null!;
        public virtual Product? Product { get; set; }
    }
}
