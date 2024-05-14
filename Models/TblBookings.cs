using System;
using System.Collections.Generic;

namespace BlissfulHomes.Models
{
    public partial class TblBookings
    {
        public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public DateTime DateBooked { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime BookingTime { get; set; }
        public int ServiceTypeId { get; set; }
        public int? NoOfBedrooms { get; set; }
        public int? NoOfBathrooms { get; set; }
        public int? NoOfKitchen { get; set; }
        public int? NoOfLivingAreas { get; set; }
        public int? Equipment { get; set; }
        public string? RegCleaningFreq { get; set; }
        public decimal? RegCleaningDiscount { get; set; }
        public string? BookingDateFlexibility { get; set; }
        public int? AccessCustomerAtHome { get; set; }
        public int? AccessCustomerWillLeaveKey { get; set; }
        public int? AccessCustomerBuildingAccessCode { get; set; }
        public string? AccessOther { get; set; }
        public int? ParkingInDriveway { get; set; }
        public int? ParkingFreeUnlimitedStreet { get; set; }
        public int? ParkingFreeMetered { get; set; }
        public int? ParkingVisitors { get; set; }
        public int? ParkingPaid { get; set; }
        public int? Pets { get; set; }
        public int? Furniture { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? BookingStatus { get; set; }
        public string? BookedBy { get; set; }
        public string? BookingMethod { get; set; }
        public DateTime? AssignedOn { get; set; }
        public string? PaymentStatus { get; set; }
        public int? Reminder { get; set; }
        public string? StreetNo { get; set; }
        public string? StreetName { get; set; }
        public string? State { get; set; }
        public string? Suburb { get; set; }
        public string? PostCode { get; set; }
        public string? DiscountCode { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
    }
}
