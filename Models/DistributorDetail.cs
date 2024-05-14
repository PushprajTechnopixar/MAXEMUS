using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class DistributorDetail
    {
        public DistributorDetail()
        {
            CartDetail = new HashSet<CartDetail>();
            DealerDetail = new HashSet<DealerDetail>();
            DealerProduct = new HashSet<DealerProduct>();
            DistributorAddress = new HashSet<DistributorAddress>();
            OderAddress = new HashSet<OderAddress>();
        }

        public int DistributorId { get; set; }
        public string? UserId { get; set; }
        public int? AddressId { get; set; }
        public string Name { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string Status { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public string? Email { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string? DistributorCode { get; set; }

        public virtual DistributorAddress? Address { get; set; }
        public virtual ICollection<CartDetail> CartDetail { get; set; }
        public virtual ICollection<DealerDetail> DealerDetail { get; set; }
        public virtual ICollection<DealerProduct> DealerProduct { get; set; }
        public virtual ICollection<DistributorAddress> DistributorAddress { get; set; }
        public virtual ICollection<OderAddress> OderAddress { get; set; }
    }
}
