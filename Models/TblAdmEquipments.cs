using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmEquipments
    {
        public int EquipmentId { get; set; }
        public string? Equipment { get; set; }
        public int? IsActive { get; set; }
    }
}
