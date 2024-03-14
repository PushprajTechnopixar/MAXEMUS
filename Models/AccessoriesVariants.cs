using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class AccessoriesVariants
    {
        public int? ProductId { get; set; }
        public int? AccessoryId { get; set; }

        public virtual Product? Product { get; set; }
    }
}
