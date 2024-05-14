using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmDiscountCodes
    {
        public long DiscountCodeId { get; set; }
        public string DiscountCode { get; set; } = null!;
        public string? DiscountType { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public long? IsActive { get; set; }
        public decimal? PercentageDiscount { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
