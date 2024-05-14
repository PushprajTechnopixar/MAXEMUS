using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models1
{
    public partial class PowerVariants
    {
        public int VariantId { get; set; }
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }
        public int? ProductId { get; set; }
    }
}
