using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmAreas
    {
        public long AreaId { get; set; }
        public string Area { get; set; } = null!;
        public string State { get; set; } = null!;
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
