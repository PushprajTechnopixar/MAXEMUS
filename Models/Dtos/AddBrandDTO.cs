using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaxemusAPI.Models
{

    public class AddBrandDTO
    {
        public int BrandId { get; set; }
        [Required] public string? BrandName { get; set; }
    }

    public class BrandDTO
    {
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandImage { get; set; }
        public string CreateDate { get; set; }
        public string ModifyDate { get; set; }
    }

    public class BrandListDTO
    {
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandImage { get; set; }
        public string CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }

    }
    public class AddOrUpdateRewardProductDTo
    {
        public int? RewardProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double Mrp { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public int NeededPointToRedeem { get; set; }
        public DateTime ExpiryDate { get; set; }

    }

    public class RewardproductListDTO
    {
        public int? RewardProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public double Mrp { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public int NeededPointToRedeem { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}
