using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblProviderServices
    {
        public long ProviderServiceId { get; set; }
        public int ProviderId { get; set; }
        public int? CleaningTypeId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
