using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class SubCategory
    {
        public SubCategory()
        {
            Product = new HashSet<Product>();
        }

        public int SubCategoryId { get; set; }
        public int MainCategoryId { get; set; }
        public string SubCategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string? SubCategoryImage { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public virtual MainCategory MainCategory { get; set; } = null!;
        public virtual ICollection<Product> Product { get; set; }
    }
}
