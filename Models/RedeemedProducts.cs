using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class RedeemedProducts
    {
        public int ReedemProductId { get; set; }
        public int RewardProductId { get; set; }
        public int? ProductCount { get; set; }
        public int? ReedemedPoint { get; set; }
        public DateTime CreateDate { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }

        public virtual RewardProduct RewardProduct { get; set; } = null!;
    }
}
