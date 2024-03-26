using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class DistributorDetailDTO
    {
        public int DistributorId { get; set; }
        public string AddressType { get; set; } 
        //public string Name { get; set; } 
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class DistributorAddressResponseDTO
    {
        public int DistributorId { get; set; }
        public int AddressId { get; set; }
        public string Name { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string AddressType { get; set; } = null!;
        public int CountryId { get; set; }
        public string CountryName{ get; set; }
        public int StateId { get; set; }
        public string StateName{ get; set; }
        public string City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string CreateDate { get; set; }
    }
    public class SetDistributorStatusDTO
    {
        public int distributorId { get; set; }
        public int status { get; set; }
    }

    public class DistributorDetailsDTO
    {
        public string id { get; set; }
        public int DistributorId { get; set; }
        public int AddressId { get; set; }
        public string Name { get; set; }
        public string email { get; set; }
        public string? Image { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string AddressType { get; set; } = null!;
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string CreateDate { get; set; }

        public string? gender { get; set; }
 
        public string Status { get; set; }

    }
}
