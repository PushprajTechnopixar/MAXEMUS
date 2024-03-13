using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class DistributorDetail
    {
        public int DistributorId { get; set; }
        public string? UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        public string? Landmark { get; set; }
        public int? StateId { get; set; }
        public string City { get; set; } = null!;
        public string PinCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AddressLatitude { get; set; }
        public string? AddressLongitude { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
