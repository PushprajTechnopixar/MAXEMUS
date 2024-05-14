using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models
{
    public partial class ContactUs
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
