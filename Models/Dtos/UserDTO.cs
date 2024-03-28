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
        public string id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string profilePic { get; set; }
        public string? gender { get; set; }
        public string Status { get; set; }
        public string createDate { get; set; }
    }
}
