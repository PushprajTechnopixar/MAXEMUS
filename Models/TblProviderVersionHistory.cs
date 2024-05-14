using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblProviderVersionHistory
    {
        public long ProviderId { get; set; }
        public string ProviderFname { get; set; } = null!;
        public string ProviderLname { get; set; } = null!;
        public string ProviderEmail { get; set; } = null!;
        public string ProviderMobile { get; set; } = null!;
        public string ProviderSuburb { get; set; } = null!;
        public string ProviderPostCode { get; set; } = null!;
        public string ProviderAbn { get; set; } = null!;
        public string ProviderEmergencyContactName { get; set; } = null!;
        public string ProviderEmergencyContactNumber { get; set; } = null!;
        public string? ProviderCompanyName { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int? ProviderTypeId { get; set; }
    }
}
