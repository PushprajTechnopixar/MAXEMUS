using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class InstallationDocumentVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string InstallationDocument { get; set; } = null!;

        public virtual Product Product { get; set; } = null!;
    }
}
