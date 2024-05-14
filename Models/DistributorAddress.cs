using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class DistributorAddress
    {
        public DistributorAddress()
        {
            DistributorDetail = new HashSet<DistributorDetail>();
        }

        public int AddressId { get; set; }
        public int DistributorId { get; set; }
        public string AddressType { get; set; } = null!;
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? BuildingNameOrNumber { get; set; }

        public virtual CountryMaster? Country { get; set; }
        public virtual DistributorDetail Distributor { get; set; } = null!;
        public virtual StateMaster? State { get; set; }
        public virtual ICollection<DistributorDetail> DistributorDetail { get; set; }
    }
}
