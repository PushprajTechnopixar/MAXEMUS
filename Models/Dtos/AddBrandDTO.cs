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
}
