using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmServiceType
    {
        public int ServiceTypeId { get; set; }
        public string? ServiceType { get; set; }
        public string? ServiceDiscription { get; set; }
        public string? IconImage { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
