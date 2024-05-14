using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class CertificationVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Certifications { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
