using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmCleaningType
    {
        public int CleaningTypeId { get; set; }
        public string? CleaningType { get; set; }
        public int? IsActive { get; set; }
    }
}
