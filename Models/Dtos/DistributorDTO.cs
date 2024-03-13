using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models.Dtos
{
    public class DistributorDTO
    {

        [Required][EmailAddress] public string Email { get; set; }
        [Required] public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required] public string Gender { get; set; }
        [Required] public string DialCode { get; set; }
        [Required] public string PhoneNumber { get; set; }
        public string DeviceType { get; set; }
        public int CountryId { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Landmark { get; set; }
        public int StateId { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public string AddressLatitude { get; set; }
        public string AddressLongitude { get; set; }
        [Required] public string Password { get; set; }
        //[Required] public string Role { get; set; }
    }
}
