using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmRates
    {
        public long RateId { get; set; }
        public int ServiceId { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal PlacementCommissionHourlyRate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal? WeeklyDiscount { get; set; }
        public decimal? FornightlyDiscount { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal? HourlyRateWoequipment { get; set; }
        public decimal? PlacementCommissionHourlyRateWoequipment { get; set; }
    }
}
