namespace MaxemusAPI.Models.Dtos
{
    public class UserDTO
    {
        public string id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string profilepic { get; set; }
        public string? gender { get; set; }
        public string? dialCode { get; set; }
        public string phoneNumber { get; set; }
        public string role { get; set; }
    }

    public class DistributorUserListDTO
    {
        public int distributorId { get; set; }
        public string userId { get; set; }
        public string distributorCode { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string profilePic { get; set; }
        public string? phoneNumber { get; set; }
        public string? BuildingNameOrNumber { get; set; }
        public string? streetAddress { get; set; }
        public string? gender { get; set; }
        public string Status { get; set; }
        public string createDate { get; set; }
    }

    public class DistributorDetailForDealerDTO
    {
        public string distributorCode { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string? phoneNumber { get; set; }
        public string? BuildingNameOrNumber { get; set; }
        public string? streetAddress { get; set; }
        public string email { get; set; }

    }
}
