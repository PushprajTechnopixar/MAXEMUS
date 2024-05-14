using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class AudioVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? BuiltInMic { get; set; }
        public string? AudioCompression { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
