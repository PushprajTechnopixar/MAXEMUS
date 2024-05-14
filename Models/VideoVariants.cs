using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class VideoVariants
    {
        public int VariantId { get; set; }
        public string? Compression { get; set; }
        public string? SmartCodec { get; set; }
        public string? VideoFrameRate { get; set; }
        public string? StreamCapability { get; set; }
        public string? Resolution { get; set; }
        public string? BitRateControl { get; set; }
        public string? VideoBitRate { get; set; }
        public string? DayNight { get; set; }
        public string? Blc { get; set; }
        public string? Hlc { get; set; }
        public string? Wdr { get; set; }
        public string? WhiteBalance { get; set; }
        public string? GainControl { get; set; }
        public string? NoiseReduction { get; set; }
        public string? MotionDetection { get; set; }
        public string? RegionOfInterest { get; set; }
        public string? SmartIr { get; set; }
        public string? ImageRotation { get; set; }
        public string? Mirror { get; set; }
        public string? PrivacyMasking { get; set; }
        public int ProductId { get; set; }
    }
}
