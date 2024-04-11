using System;
using System.Collections.Generic;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class DealerDetail
    {
        public DealerDetail()
        {
            DealerProduct = new HashSet<DealerProduct>();
        }

        public int DealerId { get; set; }
        public string UserId { get; set; } = null!;
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual ICollection<DealerProduct> DealerProduct { get; set; }
    }
}
