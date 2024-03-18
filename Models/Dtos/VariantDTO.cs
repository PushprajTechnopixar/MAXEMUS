namespace MaxemusAPI.Models.Dtos
{
    public class AccessoriesVariantsDTO
    {
        public int? ProductId { get; set; }
        public int? AccessoryId { get; set; }
    }

    public class AudioVariantsDTO
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? BuiltInMic { get; set; }
        public string? AudioCompression { get; set; }
    }

    public class CameraVariantsDTO
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
    }
    public class CertificationVariantsDTO
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Certifications { get; set; }
    }

    public partial class EnvironmentVariantsDTO
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? OperatingConditions { get; set; }
        public string? StorageTemperature { get; set; }
        public string? Protection { get; set; }
    }

    public class GeneralVariantsDTO
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? CasingMetalPlastic { get; set; }
        public string? Dimensions { get; set; }
        public string? NetWeight { get; set; }
        public string? GrossWeight { get; set; }

    }

    public class InstallationDocumentVariantsDTO
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public byte[] PdfLink { get; set; } = null!;


    }
    public class LensVariantsDTO
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
    public class NetworkVariantsDTO
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
    }

    public class PowerVariantsDTO
    {
        public int VariantId { get; set; }
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }
    }
    public class VideoVariantsDTO
    {
        public int? VariantId { get; set; }
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
    }

    public class ProductVariantDTO
    {
        //public int VariantId { get; set; }
        //public int ProductId { get; set; }
        //public DateTime CreateDate { get; set; }

        //Product
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Accessories
      //  public int? AccessoryId { get; set; }

        // Audio
        public string? BuiltInMic { get; set; }
        public string? AudioCompression { get; set; }

        // Camera
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

        // Certification
        public string? Certifications { get; set; }

        // Environment
        public string? OperatingConditions { get; set; }
        public string? StorageTemperature { get; set; }
        public string? Protection { get; set; }

        // General
        public string? CasingMetalPlastic { get; set; }
        public string? Dimensions { get; set; }
        public string? NetWeight { get; set; }
        public string? GrossWeight { get; set; }

        // Installation Document
     //   public byte[] PdfLink { get; set; }

        // Lens
        public string? LensType { get; set; }
        public string? MountType { get; set; }
        public string? FocalLength { get; set; }
        public string? MaxAperture { get; set; }
        public string? FieldOfView { get; set; }
        public string? IrisType { get; set; }
        public string? CloseFocusDistance { get; set; }
        public string? Doridistance { get; set; }

        // Network
        public string? Protocol { get; set; }
        public string? Interoperability { get; set; }
        public int? UserHost { get; set; }
        public string? EdgeStorage { get; set; }
        public string? Browser { get; set; }
        public string? ManagementSoftware { get; set; }
        public string? MobilePhone { get; set; }

        // Power
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }

        // Video
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
    }

}