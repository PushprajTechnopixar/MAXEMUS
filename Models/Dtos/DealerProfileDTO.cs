namespace MaxemusAPI.Models.Dtos
{
    public class DealerProfileDTO
    {
        public int DealerId { get; set; }
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }

    }
}
