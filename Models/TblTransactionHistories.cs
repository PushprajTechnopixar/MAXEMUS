using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblTransactionHistories
    {
        public long TransactionId { get; set; }
        public long InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? StripeChargeId { get; set; }
        public string? PaymentNote { get; set; }
        public long? CustomerId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
    }
}
