using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblCustomer
    {
        public long CustomerId { get; set; }
        public string CustomerFname { get; set; } = null!;
        public string CustomerLname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Mobile { get; set; } = null!;
        public DateTime JoinedOn { get; set; }
        public string? PasswordHash { get; set; }
    }
}
