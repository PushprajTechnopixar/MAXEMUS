using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models1
{
    public partial class CountryMaster
    {
        public CountryMaster()
        {
            AspNetUsers = new HashSet<AspNetUsers>();
            DistributorAddress = new HashSet<DistributorAddress>();
            OderAddress = new HashSet<OderAddress>();
        }

        public int CountryId { get; set; }
        public string CountryName { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string Timezone { get; set; } = null!;

        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }
        public virtual ICollection<DistributorAddress> DistributorAddress { get; set; }
        public virtual ICollection<OderAddress> OderAddress { get; set; }
    }
}
