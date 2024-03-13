using System.ComponentModel.DataAnnotations;

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
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        [Required] public string password { get; set; }
     //   [Required] public string role { get; set; }
    }
}
