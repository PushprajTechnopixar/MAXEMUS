﻿using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class DistributorAddressDTO
    {
        public string AddressType { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class DistributorBusinessRequestDTO
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public string City { get; set; }
        public string? HouseNoOrBuildingName { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class DistributorRequestDTO
    {
        public int DistributorId { get; set; }
        public UserRequestDTO personalProfile { get; set; }
        public DistributorBusinessRequestDTO businessProfile { get; set; }
    }


    public class SetDistributorStatusDTO
    {
        public int distributorId { get; set; }
        public int status { get; set; }
    }
    public class UserResponseDTO
    {
        public int? AddressId { get; set; }
        public string email { get; set; } 
        public string firstName { get; set; }
        public string lastName { get; set; }   
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string phoneNumber { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
    public class DistributorDetailsDTO
    {
        public string? UserId { get; set; }
        public UserResponseDTO personalProfile { get; set; }
        public DistributorBusinessResponseDTO businessProfile { get; set; }

    }
    public class DistributorBusinessResponseDTO
    {
        public int DistributorId { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
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
    }
}
