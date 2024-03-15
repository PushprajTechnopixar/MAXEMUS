using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class DistributorDetailDTO
    {
        public int DistributorId { get; set; }
        public string Name { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    public class DistributorAddressDTO
    {
        public int AddressId { get; set; }
        public int DistributorId { get; set; }
        public string AddressType { get; set; } = null!;
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
    }

    public class DistributorAddressResponseDTO
    {
        public int AddressId { get; set; }
        public int DistributorId { get; set; }
        public string AddressType { get; set; } = null!;
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int? City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
