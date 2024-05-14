using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class DealerProduct
    {
        public int DealerProductId { get; set; }
        public int DealerId { get; set; }
        public int DistributorId { get; set; }
        public int ProductId { get; set; }
        public int ProductStockId { get; set; }
        public int? RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Status { get; set; }

        public virtual DealerDetail Dealer { get; set; } = null!;
        public virtual DistributorDetail Distributor { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ProductStock ProductStock { get; set; } = null!;
    }
}
