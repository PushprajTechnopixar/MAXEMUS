using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class RewardProduct
    {
        public RewardProduct()
        {
            RedeemedProducts = new HashSet<RedeemedProducts>();
            ReedemProducts = new HashSet<ReedemProducts>();
        }

        public int RewardProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public double Mrp { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public int NeededPointToRedeem { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual ICollection<RedeemedProducts> RedeemedProducts { get; set; }
        public virtual ICollection<ReedemProducts> ReedemProducts { get; set; }
    }
}
