namespace MaxemusAPI.Models.Dtos
{
    public class AdminCompanyDTO
    {
        public string companyName { get; set; } = null!;
        public string? registrationNumber { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string? city { get; set; }
        public string? Timing { get; set; }
        public string? TwitterLink { get; set; }
        public string? facebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? BuildingNameOrNumber { get; set; }
        public string? streetAddress { get; set; }
        public string? landmark { get; set; }
        public string? postalCode { get; set; }
        public string? phoneNumber { get; set; }
        public string? whatsAppNumber { get; set; }
        public string? aboutUs { get; set; }
    }

    public class AdminCompanyResponseDTO
    {
        public int CompanyId { get; set; }
        public string? UserId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? RegistrationNumber { get; set; }
        public string? Timing { get; set; }
        public string? TwitterLink { get; set; }
        public string? facebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? Image { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string? countryName { get; set; }
        public string? stateName { get; set; }
        public string? City { get; set; }
        public string? BuildingNameOrNumber { get; set; }
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
        public string? userId { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string? phoneNumber { get; set; }
        public string? profilePic { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public string? countryName { get; set; }
        public string? postalCode { get; set; }
        public string? stateName { get; set; }
        public string? City { get; set; }
        public AdminCompanyResponseDTO? companyProfile { get; set; }

    }

    public class DashboardDTO
    {
        public DashboardProductViewModel latestLaunches { get; set; }
        public DashboardProductViewModel popular { get; set; }
        public AdminResponseDTO companyDetail { get; set; }

    }
    public class DashboardProductViewModel
    {
        public string productType { get; set; }
        public int mainCategoryId { get; set; }
        public int subCategoryId { get; set; }
        public List<DashboardProductDTO> products { get; set; }

    }

}
