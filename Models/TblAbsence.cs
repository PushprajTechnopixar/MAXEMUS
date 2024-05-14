using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAbsence
    {
        public long AbsentId { get; set; }
        public long ProviderId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
