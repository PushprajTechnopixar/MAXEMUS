using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class Points
    {
        public int PointId { get; set; }
        public string UserId { get; set; } = null!;
        public double? ActivePoints { get; set; }
        public double? RedeemedPoints { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
