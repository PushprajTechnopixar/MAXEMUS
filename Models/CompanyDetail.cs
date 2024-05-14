using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class CompanyDetail
    {
        public int CompanyId { get; set; }
        public string? UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Image { get; set; }
        public string? Timing { get; set; }
        public string? TwitterLink { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string? City { get; set; }
        public string? BuildingNameOrNumber { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? AboutUs { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
