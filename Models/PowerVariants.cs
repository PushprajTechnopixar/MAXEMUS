using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class PowerVariants
    {
        public int VariantId { get; set; }
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }
    }
}
