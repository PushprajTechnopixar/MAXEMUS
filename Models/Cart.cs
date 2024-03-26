using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class Cart
    {
        public int CartId { get; set; }
        public int? ProductId { get; set; }
        public string DistributorId { get; set; } = null!;
        public int? ProductCountInCart { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual Product? Product { get; set; }
    }
}
