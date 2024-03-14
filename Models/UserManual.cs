using System;
using System.Collections.Generic;
using MaxemusAPI;
using MaxemusAPI.Models;


namespace MaxemusAPI.Models
{
    public partial class UserManual
    {
        public int? ProductId { get; set; }
        public string? PdfLink { get; set; }

        public virtual Product? Product { get; set; }
    }
}
