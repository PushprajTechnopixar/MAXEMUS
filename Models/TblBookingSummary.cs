using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblBookingSummary
    {
        public long BookingAccountId { get; set; }
        public long BookingId { get; set; }
        public int ServiceTypeId { get; set; }
        public decimal ServiceTotal { get; set; }
        public decimal? ExtrasTotal { get; set; }
        public decimal? RegCleaningDiscount { get; set; }
        public decimal? DiscountCodeAmount { get; set; }
        public decimal? BookingTotal { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
