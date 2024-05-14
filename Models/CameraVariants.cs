using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class CameraVariants
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Appearance { get; set; }
        public string? ImageSensor { get; set; }
        public string? EffectivePixels { get; set; }
        public string? Rom { get; set; }
        public string? Ram { get; set; }
        public string? ScanningSystem { get; set; }
        public string? ElectronicShutterSpeed { get; set; }
        public string? MinIllumination { get; set; }
        public string? Irdistance { get; set; }
        public string? IronOffControl { get; set; }
        public string? IrledsNumber { get; set; }
        public string? PanTiltRotationRange { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
