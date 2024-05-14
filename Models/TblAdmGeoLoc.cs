using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblAdmGeoLoc
    {
        public long PostCodeId { get; set; }
        public string? PostCode { get; set; }
        public string? Locality { get; set; }
        public string? State { get; set; }
        public string? Category { get; set; }
        public string? Longitude { get; set; }
        public string? Latittude { get; set; }
    }
}
