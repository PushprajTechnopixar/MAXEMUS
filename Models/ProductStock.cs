using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class ProductStock
    {
        public ProductStock()
        {
            DealerProduct = new HashSet<DealerProduct>();
        }

        public int ProductStockId { get; set; }
        public int ProductId { get; set; }
        public string SerialNumber { get; set; } = null!;
        public string? Qrcode { get; set; }
        public int RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string? Status { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<DealerProduct> DealerProduct { get; set; }
    }
}
