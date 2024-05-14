using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmServicePricing
    {
        public long PricingId { get; set; }
        public int NoOfBedrooms { get; set; }
        public int NoOfBathrooms { get; set; }
        public int? NoOfLivingAreas { get; set; }
        public int? NoOfKitchen { get; set; }
        public int ServiceId { get; set; }
        public string? Chore { get; set; }
        public bool? IsActive { get; set; }
        public string? ToolTip { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? Placementhours { get; set; }
    }
}
