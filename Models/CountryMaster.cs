using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class CountryMaster
    {
        public CountryMaster()
        {
            DistributorAddress = new HashSet<DistributorAddress>();
            OderAddress = new HashSet<OderAddress>();
        }

        public int CountryId { get; set; }
        public string CountryName { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string Timezone { get; set; } = null!;

        public virtual ICollection<DistributorAddress> DistributorAddress { get; set; }
        public virtual ICollection<OderAddress> OderAddress { get; set; }
    }
}
