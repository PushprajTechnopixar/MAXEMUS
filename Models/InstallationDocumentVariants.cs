using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class InstallationDocumentVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public byte[] PdfLink { get; set; } = null!;

        public virtual Product Product { get; set; } = null!;
    }
}
