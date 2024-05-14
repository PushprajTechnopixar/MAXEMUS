using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class NetworkVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Protocol { get; set; }
        public string? Interoperability { get; set; }
        public int? UserHost { get; set; }
        public string? EdgeStorage { get; set; }
        public string? Browser { get; set; }
        public string? ManagementSoftware { get; set; }
        public string? MobilePhone { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
