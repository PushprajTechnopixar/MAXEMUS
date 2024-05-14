using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class AccessoriesVariants
    {
        public int ProductId { get; set; }
        public int AccessoryId { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
