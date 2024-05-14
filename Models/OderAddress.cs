using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class OderAddress
    {
        public int AddressId { get; set; }
        public int DistributorId { get; set; }
        public string AddressType { get; set; } = null!;
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual CountryMaster Country { get; set; } = null!;
        public virtual DistributorDetail Distributor { get; set; } = null!;
        public virtual StateMaster State { get; set; } = null!;
    }
}
