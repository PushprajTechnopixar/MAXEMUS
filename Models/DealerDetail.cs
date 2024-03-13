using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MaxemusAPI;
using MaxemusAPI.Models;

namespace MaxemusAPI.Models
{
    public partial class DealerDetail
    {
        [Key] public int DealerId { get; set; }
        public string? UserId { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string Status { get; set; } 
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
