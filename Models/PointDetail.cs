using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class PointDetail
    {
        public int PointId { get; set; }
        public string UserId { get; set; } = null!;
        public double? Points { get; set; }
        public int? RedeemedPoints { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DealerProductId { get; set; }

    }
}
