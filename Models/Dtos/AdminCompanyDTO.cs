namespace MaxemusAPI.Models.Dtos
{
    public class AdminCompanyDTO
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Image { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? AboutUs { get; set; }
    }

    public class AdminCompanyResponseDTO
    {
        public int CompanyId { get; set; }
        public string? UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Image { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string? WhatsappNumber { get; set; }
        public string? AboutUs { get; set; }
    }
    public class AdminResponseDTO
    {
        public int CompanyId { get; set; }
        public string? UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Gender { get; set; }
        public string? DialCode { get; set; }
        public string? ProfilePic { get; set; }
        public string? PAN { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Image { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? AboutUs { get; set; }

    }
}
