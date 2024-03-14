using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class DealerDetail
    {
        public int DealerId { get; set; }
        public string? UserId { get; set; }
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
