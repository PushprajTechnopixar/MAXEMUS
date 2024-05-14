using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblPlacement
    {
        public long PlacementId { get; set; }
        public long CustomerId { get; set; }
        public long BookingId { get; set; }
        public int ServiceTypeId { get; set; }
        public long ProviderId { get; set; }
        public string CustomerFname { get; set; } = null!;
        public string CustomerLname { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerMobile { get; set; } = null!;
        public string CustomerSuburb { get; set; } = null!;
        public string CustomerPostCode { get; set; } = null!;
        public string CustomerStreetNo { get; set; } = null!;
        public string CustomerStreetName { get; set; } = null!;
        public DateTime JobDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public decimal? BookingEstimatedHours { get; set; }
        public decimal? PlacementEstimatedHours { get; set; }
        public string Status { get; set; } = null!;
        public DateTime BookedOn { get; set; }
        public string BookingMethod { get; set; } = null!;
        public string BookedBy { get; set; } = null!;
        public DateTime? AssignedOn { get; set; }
    }
}
