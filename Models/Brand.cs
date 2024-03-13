using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class Brand
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public string? BrandImage { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
