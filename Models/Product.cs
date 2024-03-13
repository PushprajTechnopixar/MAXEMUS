using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class Product
    {
        public int ProductId { get; set; }
        public string? Model { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
