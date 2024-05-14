using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class EnvironmentVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? OperatingConditions { get; set; }
        public string? StorageTemperature { get; set; }
        public string? Protection { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
