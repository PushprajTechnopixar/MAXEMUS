using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblProviderAvailability
    {
        public long ProviderAvailibilityId { get; set; }
        public long ProviderId { get; set; }
        public int? Sunday { get; set; }
        public int? Monday { get; set; }
        public int? Tuesday { get; set; }
        public int? Wednesday { get; set; }
        public int? Thursday { get; set; }
        public int? Friday { get; set; }
        public int? Saturday { get; set; }
        public int? Morning7to12 { get; set; }
        public int? Afternoons125 { get; set; }
        public int? After6m { get; set; }
    }
}
