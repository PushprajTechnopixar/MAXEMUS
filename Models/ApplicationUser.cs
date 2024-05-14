using Microsoft.AspNetCore.Identity;

namespace MaxemusAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? DialCode { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? DeviceType { get; set; }
        public bool IsDeleted { get; set; }

        public string? ProfilePic { get; set; }
    }
}
