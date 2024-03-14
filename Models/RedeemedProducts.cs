using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class RedeemedProducts
    {
        public int ReedemProductId { get; set; }
        public int PointId { get; set; }
        public int? ProductId { get; set; }
        public int? ProductCount { get; set; }
        public int? ReedemedPoint { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual SellingPoints Point { get; set; } = null!;
        public virtual Product? Product { get; set; }
    }
}
