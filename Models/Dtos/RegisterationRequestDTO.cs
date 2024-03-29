using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class RegisterationRequestDTO
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
        public string? deviceType { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string role { get; set; }
    }
    public class UserRequestDTO
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
        public string? deviceType { get; set; }

        public string? dialCode { get; set; }
        [Required]
        public string password { get; set; }
        [Required]

        public string phoneNumber { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }


    public class AdminProfileRequestDTO
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
        public AdminCompanyDTO companyProfile { get; set; }

    }

    public class UpdateLiveLocationDTO
    {
        public string addressLongitude { get; set; }
        public string addressLatitude { get; set; }

    }

    public class PhoneLoginRequestDTO
    {

        [Required]
        public string dialCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string role { get; set; }
        public string? deviceType { get; set; }
        public string? fcmtoken { get; set; }
        public string? addressLongitude { get; set; }
        public string? addressLatitude { get; set; }
    }





}
