using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class MainCategory
    {
        public MainCategory()
        {
            Product = new HashSet<Product>();
            SubCategory = new HashSet<SubCategory>();
        }

        public int MainCategoryId { get; set; }
        public string MainCategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public string? MainCategoryImage { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public virtual ICollection<Product> Product { get; set; }
        public virtual ICollection<SubCategory> SubCategory { get; set; }
    }
}
