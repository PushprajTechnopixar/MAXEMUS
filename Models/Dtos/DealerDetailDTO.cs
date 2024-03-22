﻿using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class DealerDetailDTO
    {
        [Required][EmailAddress] public string email { get; set; }
        [Required] public string firstName { get; set; }
        public string lastName { get; set; }
        [Required] public string? gender { get; set; }
        [Required] public string? dialCode { get; set; }
        [Required] public string phoneNumber { get; set; }
        public string? deviceType { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        [Required] public string password { get; set; }
     //   [Required] public string role { get; set; }
    }
    public class DealerProfileDTO
    {
        public int DealerId { get; set; }
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }

    }
    public class DealerRequestDTO
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        public string firstName { get; set; }
        public string lastName { get; set; }
        [Required]
        public string? gender { get; set; }
        [Required]
        public string? dialCode { get; set; }
        [Required]
        public string phoneNumber { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        [Required] public string Address1 { get; set; }
        [Required] public string? Address2 { get; set; }
    }
    public class SetDealerStatusDTO
    {
        public int dealerId { get; set; }
        public int status { get; set; }
    }

    public class DealerResponseDTO
    {
        public string Id { get; set; }
        public string email { get; set; }
       
        public string firstName { get; set; }
        public string lastName { get; set; }
       
        public string? gender { get; set; }
      
        public string? dialCode { get; set; }
     
        public string phoneNumber { get; set; }
        public int countryId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public int stateId { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
       public string Address1 { get; set; }
        public string? Address2 { get; set; }
    }
}
