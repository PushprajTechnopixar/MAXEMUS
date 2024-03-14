using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class LensVariants
    {
        public int VariantId { get; set; }
        public string? LensType { get; set; }
        public string? MountType { get; set; }
        public string? FocalLength { get; set; }
        public string? MaxAperture { get; set; }
        public string? FieldOfView { get; set; }
        public string? IrisType { get; set; }
        public string? CloseFocusDistance { get; set; }
        public string? Doridistance { get; set; }
    }
}
