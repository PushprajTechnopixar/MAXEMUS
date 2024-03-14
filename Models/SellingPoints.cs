using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class SellingPoints
    {
        public SellingPoints()
        {
            RedeemedProducts = new HashSet<RedeemedProducts>();
        }

        public int PointId { get; set; }
        public string UserId { get; set; } = null!;
        public int? SellingPoints1 { get; set; }
        public int? RedeemedPoints { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateDate { get; set; }

        public virtual ICollection<RedeemedProducts> RedeemedProducts { get; set; }
    }
}
