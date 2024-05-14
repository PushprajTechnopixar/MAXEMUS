using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class UserManual
    {
        public int? ProductId { get; set; }
        public string? Mannual { get; set; }
        public int MannualId { get; set; }

        public virtual Product? Product { get; set; }
    }
}
