using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class ProductStock
    {
        public int ProductStockId { get; set; }
        public int ProductId { get; set; }
        public string SerialNumber { get; set; } = null!;
        public string? Qrcode { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
