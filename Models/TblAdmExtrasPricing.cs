using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmExtrasPricing
    {
        public long ExtrasPricingId { get; set; }
        public string ExtrasName { get; set; } = null!;
        public long ServiceId { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string? ToolTip { get; set; }
        public string? Units { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public decimal? EstimatedHours { get; set; }
        public int? AddUnits { get; set; }
        public string? CanAddUnits { get; set; }
    }
}
