using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblProviderEquipments
    {
        public int ProviderEquipmentId { get; set; }
        public long? ProviderId { get; set; }
        public int? EquipmentId { get; set; }
    }
}
