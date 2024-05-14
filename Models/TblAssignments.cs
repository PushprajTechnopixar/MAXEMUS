using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAssignments
    {
        public long AssignmentId { get; set; }
        public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public long ServiceId { get; set; }
        public long PlacementId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? BookedOn { get; set; }
        public DateTime? AssignedOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public DateTime? DeclinedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public DateTime? CancelledOn { get; set; }
        public string? Admin { get; set; }
        public string? Cleaner { get; set; }
    }
}
