using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblReviews
    {
        public long ReviewId { get; set; }
        public long? CustomerId { get; set; }
        public long? PlacementId { get; set; }
        public long? ProviderId { get; set; }
        public int? Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
