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
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string phoneNumber { get; set; }
        public string distributorCode { get; set; }
        public int? countryId { get; set; }
        public int? stateId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
    }
    public class SetDealerStatusDTO
    {
        public int dealerId { get; set; }
        public int distributorId { get; set; }
        public string status { get; set; }
    }
    public class ScanProductDTO
    {
        public String serialNumber { get; set; }
    }


    public class UserDetailRequestDTO
    {
        [Required]
        [EmailAddress]
        public string? email { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string phoneNumber { get; set; }
        public int? countryId { get; set; }
        public int? stateId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
    }

    public class DealerResponseDTO
    {
        public string Id { get; set; }
        public string distributorCode { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string? ProfilePic { get; set; }
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
        public string Status { get; set; }
        public DistributorDetailForDealerDTO? distributor { get; set; }
    }



    public class UserDetailResponseDTO
    {
        public int userDetailId { get; set; }
        public string userId { get; set; }
        public string? email { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string? phoneNumber { get; set; }
        public int? countryId { get; set; }
        public string? CountryName { get; set; }
        public string? StateName { get; set; }
        public int? stateId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
    }

    public class DealerUserListDTO
    {
        public int dealerId { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
        public string distributorCode { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string profilePic { get; set; }
        public string? gender { get; set; }
        public string Status { get; set; }
        public string createDate { get; set; }
        public string? distributorFirstName { get; set; }
        public string? distributorLastName { get; set; }
        public int distributorId { get; set; }

    }
    public class DealerProductDTO
    {
        public int OrderedProductId { get; set; }
        public int DealerId { get; set; }
        public int DistributorId { get; set; }
        public int ProductId { get; set; }
        public int ProductStockId { get; set; }
        public int? RewardPoint { get; set; }
        public string CreateDate { get; set; }
    }
}
