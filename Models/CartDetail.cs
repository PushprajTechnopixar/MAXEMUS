using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class CartDetail
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int? DistributorId { get; set; }
        public int? UserDetailId { get; set; }
        public int? ProductCountInCart { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

        public virtual DistributorDetail? Distributor { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
