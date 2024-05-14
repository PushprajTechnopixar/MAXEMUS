using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblBookingsExtras
    {
        public long BookingExtrasId { get; set; }
        public long? BookingId { get; set; }
        public int? ExtrasPricingId { get; set; }
        public int? ServiceTypeId { get; set; }
        public long? CustomerId { get; set; }
        public decimal? Units { get; set; }
        public decimal? Price { get; set; }
    }
}
