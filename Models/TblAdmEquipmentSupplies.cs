using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmEquipmentSupplies
    {
        public int EquipmentSuppliesId { get; set; }
        public string EquipmentSupplies { get; set; } = null!;
        public string EquipmentSuppliesName { get; set; } = null!;
        public string Notes { get; set; } = null!;
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
