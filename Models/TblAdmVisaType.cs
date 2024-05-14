using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmVisaType
    {
        public int VisaTypeId { get; set; }
        public string VisaType { get; set; } = null!;
        public string? Details { get; set; }
        public decimal? HoursPerWeek { get; set; }
    }
}
