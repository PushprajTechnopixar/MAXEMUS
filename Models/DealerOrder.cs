﻿using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class DealerOrder
    {
        public DealerOrder()
        {
            DealerOrderedProduct = new HashSet<DealerOrderedProduct>();
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
        public int? TotalProducts { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CancelledBy { get; set; }
        public string? PaymentReceipt { get; set; }
        public int? RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }

      //  public virtual AspNetUsers User { get; set; } = null!;
        public virtual ICollection<DealerOrderedProduct> DealerOrderedProduct { get; set; }
    }
}
