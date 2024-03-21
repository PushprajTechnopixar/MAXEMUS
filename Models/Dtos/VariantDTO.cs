using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int RewardPoint { get; set; }
        public DateTime CreateDate { get; set; }
    }
    public class AccessoriesVariantsDTO
    {
        public int ProductId { get; set; }
        public int AccessoryId { get; set; }

    }
    public class AudioVariantsDTO
    {

        public string? BuiltInMic { get; set; }
        public string? AudioCompression { get; set; }
    }
    public class CameraVariantsDTO
    {

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

    }
    public class CertificationVariantsDTO
    {
        public string? Certifications { get; set; }

    }
    public class EnvironmentVariantsDTO
    {
        public string? OperatingConditions { get; set; }
        public string? StorageTemperature { get; set; }
        public string? Protection { get; set; }

    }
    public class GeneralVariantsDTO
    {
        public string? CasingMetalPlastic { get; set; }
        public string? Dimensions { get; set; }
        public string? NetWeight { get; set; }
        public string? GrossWeight { get; set; }

    }
    public class InstallationDocumentVariantsDTO
    {
        public byte[] PdfLink { get; set; } = null!;

    }
    public class LensVariantsDTO
    {
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
        public string? PowerSupply { get; set; }
        public string? PowerConsumption { get; set; }
    }
    public class VideoVariantsDTO
    {
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
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public List<CameraVariantsDTO> Camera { get; set; }
        public List<AudioVariantsDTO> Audio { get; set; }
        public List<LensVariantsDTO> Lens { get; set; }
        public List<CertificationVariantsDTO> Certification { get; set; }
        public List<EnvironmentVariantsDTO> Environment { get; set; }
        public List<GeneralVariantsDTO> General { get; set; }
        public List<NetworkVariantsDTO> Network { get; set; }
        public List<PowerVariantsDTO> Power { get; set; }
        public List<VideoVariantsDTO> Video { get; set; }
    }
    public class ProductResponsesDTO
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public bool? IsActive { get; set; }
        public int RewardPoint { get; set; }
        public string CreateDate { get; set; }

        public AccessoriesVariantsDTO Accessories { get; set; }
        public AudioVariantsDTO Audio { get; set; }
        public CameraVariantsDTO Camera { get; set; }
        public CertificationVariantsDTO Certification { get; set; }
        public EnvironmentVariantsDTO Environment { get; set; }
        public GeneralVariantsDTO General { get; set; }
        public InstallationDocumentVariantsDTO InstallationDocument { get; set; }
        public LensVariantsDTO Lens { get; set; }
        public NetworkVariantsDTO Network { get; set; }
        public PowerVariantsDTO Power { get; set; }
        public VideoVariantsDTO Video { get; set; }
    }   
    public class ProductUpdateDTO
    {
        public int ProductId { get; set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }

        public AudioVariantsDTO Audio { get; set; }
        public CameraVariantsDTO Camera { get; set; }
        public CertificationVariantsDTO Certification { get; set; }
        public EnvironmentVariantsDTO Environment { get; set; }
        public GeneralVariantsDTO General { get; set; }
        public LensVariantsDTO Lens { get; set; }
        public NetworkVariantsDTO Network { get; set; }
        public PowerVariantsDTO Power { get; set; }
        public VideoVariantsDTO Video { get; set; }
    }

}