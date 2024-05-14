using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class PowerVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }
    }
}
