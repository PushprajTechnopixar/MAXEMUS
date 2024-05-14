using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class StateMaster
    {
        public StateMaster()
        {
            CityMaster = new HashSet<CityMaster>();
            DistributorAddress = new HashSet<DistributorAddress>();
            OderAddress = new HashSet<OderAddress>();
        }

        public int StateId { get; set; }
        public string StateName { get; set; } = null!;
        public int CountryId { get; set; }

        public virtual ICollection<CityMaster> CityMaster { get; set; }
        public virtual ICollection<DistributorAddress> DistributorAddress { get; set; }
        public virtual ICollection<OderAddress> OderAddress { get; set; }
    }
}
