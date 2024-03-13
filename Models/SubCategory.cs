using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class SubCategory
    {
        public int SubCategoryId { get; set; }
        public int MainCategoryId { get; set; }
        public string SubCategoryName { get; set; } = null!;
        public string? SubCategoryImage { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public virtual MainCategory MainCategory { get; set; } = null!;
    }
}
