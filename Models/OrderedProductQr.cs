using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class OrderedProductQr
    {
        public int? ProductStockId { get; set; }
        public int? ProductId { get; set; }

        public virtual Product? Product { get; set; }
        public virtual ProductStock? ProductStock { get; set; }
    }
}
