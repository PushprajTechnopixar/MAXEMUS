using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class GeneralVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? CasingMetalPlastic { get; set; }
        public string? Dimensions { get; set; }
        public string? NetWeight { get; set; }
        public string? GrossWeight { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
