using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblCleaningStats
    {
        public long CleaningStatsId { get; set; }
        public int NoOfHousesCleaned { get; set; }
        public int NoOfCleaners { get; set; }
        public string? NoOfCustomers { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public int? IsCurrent { get; set; }
    }
}
