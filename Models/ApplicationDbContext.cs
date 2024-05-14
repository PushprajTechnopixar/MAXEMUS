using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BlissfulHomes.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblAbsence> TblAbsence { get; set; } = null!;
        public virtual DbSet<TblAdmAreas> TblAdmAreas { get; set; } = null!;
        public virtual DbSet<TblAdmBathrooms> TblAdmBathrooms { get; set; } = null!;
        public virtual DbSet<TblAdmBedrooms> TblAdmBedrooms { get; set; } = null!;
        public virtual DbSet<TblAdmCleaningType> TblAdmCleaningType { get; set; } = null!;
        public virtual DbSet<TblAdmDiscountCodes> TblAdmDiscountCodes { get; set; } = null!;
        public virtual DbSet<TblAdmEquipmentSupplies> TblAdmEquipmentSupplies { get; set; } = null!;
        public virtual DbSet<TblAdmEquipments> TblAdmEquipments { get; set; } = null!;
        public virtual DbSet<TblAdmExtrasPricing> TblAdmExtrasPricing { get; set; } = null!;
        public virtual DbSet<TblAdmGeoLoc> TblAdmGeoLoc { get; set; } = null!;
        public virtual DbSet<TblAdmKitchen> TblAdmKitchen { get; set; } = null!;
        public virtual DbSet<TblAdmLivingAreas> TblAdmLivingAreas { get; set; } = null!;
        public virtual DbSet<TblAdmRates> TblAdmRates { get; set; } = null!;
        public virtual DbSet<TblAdmServicePricing> TblAdmServicePricing { get; set; } = null!;
        public virtual DbSet<TblAdmServiceType> TblAdmServiceType { get; set; } = null!;
        public virtual DbSet<TblAdmSocial> TblAdmSocial { get; set; } = null!;
        public virtual DbSet<TblAdmVisaType> TblAdmVisaType { get; set; } = null!;
        public virtual DbSet<TblAdmYearsofExp> TblAdmYearsofExp { get; set; } = null!;
        public virtual DbSet<TblAssignments> TblAssignments { get; set; } = null!;
        public virtual DbSet<TblBookingSummary> TblBookingSummary { get; set; } = null!;
        public virtual DbSet<TblBookings> TblBookings { get; set; } = null!;
        public virtual DbSet<TblBookingsExtras> TblBookingsExtras { get; set; } = null!;
        public virtual DbSet<TblCleaningStats> TblCleaningStats { get; set; } = null!;
        public virtual DbSet<TblCustomer> TblCustomer { get; set; } = null!;
        public virtual DbSet<TblPlacement> TblPlacement { get; set; } = null!;
        public virtual DbSet<TblProvider> TblProvider { get; set; } = null!;
        public virtual DbSet<TblProviderArea> TblProviderArea { get; set; } = null!;
        public virtual DbSet<TblProviderAvailability> TblProviderAvailability { get; set; } = null!;
        public virtual DbSet<TblProviderEquipments> TblProviderEquipments { get; set; } = null!;
        public virtual DbSet<TblProviderServices> TblProviderServices { get; set; } = null!;
        public virtual DbSet<TblProviderVersionHistory> TblProviderVersionHistory { get; set; } = null!;
        public virtual DbSet<TblReviews> TblReviews { get; set; } = null!;
        public virtual DbSet<TblTransactionHistories> TblTransactionHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblAbsence>(entity =>
            {
                entity.HasKey(e => e.AbsentId);

                entity.ToTable("tbl_Absence");

                entity.Property(e => e.AbsentId).HasColumnName("AbsentID");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.To).HasColumnName("[To");
            });

            modelBuilder.Entity<TblAdmAreas>(entity =>
            {
                entity.HasKey(e => e.AreaId);

                entity.ToTable("tblADM_Areas");

                entity.Property(e => e.AreaId).HasColumnName("AreaID");

                entity.Property(e => e.Area).HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.State).HasMaxLength(50);
            });

            modelBuilder.Entity<TblAdmBathrooms>(entity =>
            {
                entity.HasKey(e => e.BathroomId);

                entity.ToTable("tblADM_Bathrooms");

                entity.Property(e => e.BathroomId).HasColumnName("BathroomID");
            });

            modelBuilder.Entity<TblAdmBedrooms>(entity =>
            {
                entity.HasKey(e => e.BedRoomId);

                entity.ToTable("tblADM_Bedrooms");

                entity.Property(e => e.BedRoomId).HasColumnName("BedRoomID");
            });

            modelBuilder.Entity<TblAdmCleaningType>(entity =>
            {
                entity.HasKey(e => e.CleaningTypeId);

                entity.ToTable("tblADM_CleaningType");

                entity.Property(e => e.CleaningTypeId).HasColumnName("CleaningTypeID");

                entity.Property(e => e.CleaningType).HasMaxLength(100);
            });

            modelBuilder.Entity<TblAdmDiscountCodes>(entity =>
            {
                entity.HasKey(e => e.DiscountCodeId);

                entity.ToTable("tblADM_DiscountCodes");

                entity.Property(e => e.DiscountCodeId).HasColumnName("DiscountCodeID");

                entity.Property(e => e.DiscountAmount).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.DiscountCode).HasMaxLength(50);

                entity.Property(e => e.DiscountType).HasMaxLength(50);

                entity.Property(e => e.PercentageDiscount).HasColumnType("numeric(18, 2)");
            });

            modelBuilder.Entity<TblAdmEquipmentSupplies>(entity =>
            {
                entity.HasKey(e => e.EquipmentSuppliesId);

                entity.ToTable("tblADM_EquipmentSupplies");

                entity.Property(e => e.EquipmentSuppliesId).HasColumnName("EquipmentSuppliesID");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.EquipmentSupplies)
                    .HasMaxLength(1)
                    .HasColumnName("Equipment_Supplies");

                entity.Property(e => e.EquipmentSuppliesName)
                    .HasMaxLength(50)
                    .HasColumnName("Equipment_Supplies_Name");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");
            });

            modelBuilder.Entity<TblAdmEquipments>(entity =>
            {
                entity.HasKey(e => e.EquipmentId);

                entity.ToTable("tblADM_Equipments");

                entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            });

            modelBuilder.Entity<TblAdmExtrasPricing>(entity =>
            {
                entity.HasKey(e => e.ExtrasPricingId);

                entity.ToTable("tblADM_ExtrasPricing");

                entity.Property(e => e.ExtrasPricingId).HasColumnName("ExtrasPricingID");

                entity.Property(e => e.CanAddUnits).HasMaxLength(1);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.EstimatedHours).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ExtrasName).HasMaxLength(50);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.Price).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.Units).HasMaxLength(50);
            });

            modelBuilder.Entity<TblAdmGeoLoc>(entity =>
            {
                entity.HasKey(e => e.PostCodeId);

                entity.ToTable("tblADM_GeoLoc");

                entity.Property(e => e.PostCodeId).HasColumnName("PostCodeID");

                entity.Property(e => e.Category).HasMaxLength(255);

                entity.Property(e => e.Latittude).HasMaxLength(255);

                entity.Property(e => e.Locality).HasMaxLength(255);

                entity.Property(e => e.Longitude).HasMaxLength(255);

                entity.Property(e => e.PostCode).HasMaxLength(255);

                entity.Property(e => e.State).HasMaxLength(255);
            });

            modelBuilder.Entity<TblAdmKitchen>(entity =>
            {
                entity.HasKey(e => e.KitchenId);

                entity.ToTable("tblADM_Kitchen");

                entity.Property(e => e.KitchenId).HasColumnName("KitchenID");
            });

            modelBuilder.Entity<TblAdmLivingAreas>(entity =>
            {
                entity.HasKey(e => e.LivingAreaId);

                entity.ToTable("tblADM_LivingAreas");

                entity.Property(e => e.LivingAreaId).HasColumnName("LivingAreaID");
            });

            modelBuilder.Entity<TblAdmRates>(entity =>
            {
                entity.HasKey(e => e.RateId);

                entity.ToTable("tblADM_Rates");

                entity.Property(e => e.RateId).HasColumnName("RateID");

                entity.Property(e => e.FornightlyDiscount).HasColumnType("numeric(18, 0)");

                entity.Property(e => e.HourlyRate).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.HourlyRateWoequipment)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("HourlyRateWOEquipment");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.PlacementCommissionHourlyRate).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.PlacementCommissionHourlyRateWoequipment)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("PlacementCommissionHourlyRateWOEquipment");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.WeeklyDiscount).HasColumnType("numeric(18, 0)");
            });

            modelBuilder.Entity<TblAdmServicePricing>(entity =>
            {
                entity.HasKey(e => e.PricingId);

                entity.ToTable("tblADM_ServicePricing");

                entity.Property(e => e.PricingId).HasColumnName("PricingID");

                entity.Property(e => e.Chore).HasMaxLength(100);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.EstimatedHours).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.Placementhours).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            });

            modelBuilder.Entity<TblAdmServiceType>(entity =>
            {
                entity.HasKey(e => e.ServiceTypeId);

                entity.ToTable("tblADM_ServiceType");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.IconImage).HasMaxLength(256);

                entity.Property(e => e.ModifiedBy).HasMaxLength(50);

                entity.Property(e => e.ServiceDiscription).HasMaxLength(500);

                entity.Property(e => e.ServiceType).HasMaxLength(50);
            });

            modelBuilder.Entity<TblAdmSocial>(entity =>
            {
                entity.HasKey(e => e.SocialId);

                entity.ToTable("tblADm_Social");

                entity.Property(e => e.SocialId).HasColumnName("SocialID");

                entity.Property(e => e.SocialName).HasMaxLength(100);
            });

            modelBuilder.Entity<TblAdmVisaType>(entity =>
            {
                entity.HasKey(e => e.VisaTypeId);

                entity.ToTable("tblADM_VisaType");

                entity.Property(e => e.VisaTypeId).HasColumnName("VisaTypeID");

                entity.Property(e => e.HoursPerWeek).HasColumnType("numeric(18, 0)");

                entity.Property(e => e.VisaType).HasMaxLength(50);
            });

            modelBuilder.Entity<TblAdmYearsofExp>(entity =>
            {
                entity.HasKey(e => e.CleaningExpId);

                entity.ToTable("tblADM_YearsofExp");

                entity.Property(e => e.CleaningExpId).HasColumnName("CleaningExpID");

                entity.Property(e => e.Exp).HasMaxLength(50);
            });

            modelBuilder.Entity<TblAssignments>(entity =>
            {
                entity.HasKey(e => e.AssignmentId)
                    .HasName("PK_tbl_Assignments_1");

                entity.ToTable("tbl_Assignments");

                entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");

                entity.Property(e => e.Admin).HasMaxLength(50);

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.Cleaner).HasMaxLength(50);

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.PlacementId).HasColumnName("PlacementID");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<TblBookingSummary>(entity =>
            {
                entity.HasKey(e => e.BookingAccountId);

                entity.ToTable("tbl_BookingSummary");

                entity.Property(e => e.BookingAccountId).HasColumnName("BookingAccountID");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.BookingTotal).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.DiscountCodeAmount).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ExtrasTotal).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.RegCleaningDiscount).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ServiceTotal).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");
            });

            modelBuilder.Entity<TblBookings>(entity =>
            {
                entity.HasKey(e => e.BookingId);

                entity.ToTable("tbl_Bookings");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.AccessCustomerAtHome).HasColumnName("Access_CustomerAtHome");

                entity.Property(e => e.AccessCustomerBuildingAccessCode).HasColumnName("Access_CustomerBuildingAccessCode");

                entity.Property(e => e.AccessCustomerWillLeaveKey).HasColumnName("Access_CustomerWillLeaveKey");

                entity.Property(e => e.AccessOther).HasColumnName("Access_Other");

                entity.Property(e => e.BookedBy).HasMaxLength(50);

                entity.Property(e => e.BookingDateFlexibility).HasMaxLength(50);

                entity.Property(e => e.BookingMethod).HasMaxLength(50);

                entity.Property(e => e.BookingStatus).HasMaxLength(50);

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.DiscountCode).HasMaxLength(50);

                entity.Property(e => e.ParkingFreeMetered).HasColumnName("Parking_FreeMetered");

                entity.Property(e => e.ParkingFreeUnlimitedStreet).HasColumnName("Parking_Free&UnlimitedStreet");

                entity.Property(e => e.ParkingInDriveway).HasColumnName("Parking_InDriveway");

                entity.Property(e => e.ParkingPaid).HasColumnName("Parking_Paid");

                entity.Property(e => e.ParkingVisitors).HasColumnName("Parking_Visitors");

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.PostCode).HasMaxLength(50);

                entity.Property(e => e.RegCleaningDiscount).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.RegCleaningFreq).HasMaxLength(50);

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.State).HasMaxLength(3);

                entity.Property(e => e.StreetName).HasMaxLength(100);

                entity.Property(e => e.StreetNo).HasMaxLength(50);

                entity.Property(e => e.Suburb).HasMaxLength(50);
            });

            modelBuilder.Entity<TblBookingsExtras>(entity =>
            {
                entity.HasKey(e => e.BookingExtrasId);

                entity.ToTable("tbl_BookingsExtras");

                entity.Property(e => e.BookingExtrasId).HasColumnName("BookingExtrasID");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.ExtrasPricingId).HasColumnName("ExtrasPricingID");

                entity.Property(e => e.Price).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.Units).HasColumnType("numeric(18, 2)");
            });

            modelBuilder.Entity<TblCleaningStats>(entity =>
            {
                entity.HasKey(e => e.CleaningStatsId)
                    .HasName("PK_tblADM_CleaningStats");

                entity.ToTable("tbl_CleaningStats");

                entity.Property(e => e.CleaningStatsId).HasColumnName("CleaningStatsID");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.NoOfCustomers)
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<TblCustomer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.ToTable("tbl_Customer");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.CustomerFname)
                    .HasMaxLength(50)
                    .HasColumnName("CustomerFName");

                entity.Property(e => e.CustomerLname)
                    .HasMaxLength(50)
                    .HasColumnName("CustomerLName");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.JoinedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Mobile).HasMaxLength(50);

                entity.Property(e => e.PasswordHash).HasMaxLength(256);
            });

            modelBuilder.Entity<TblPlacement>(entity =>
            {
                entity.HasKey(e => e.PlacementId)
                    .HasName("PK_tbl_Assignments");

                entity.ToTable("tbl_Placement");

                entity.Property(e => e.PlacementId).HasColumnName("PlacementID");

                entity.Property(e => e.BookedBy).HasMaxLength(50);

                entity.Property(e => e.BookingEstimatedHours).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.BookingId).HasColumnName("BookingID");

                entity.Property(e => e.BookingMethod).HasMaxLength(50);

                entity.Property(e => e.CustomerEmail).HasMaxLength(100);

                entity.Property(e => e.CustomerFname)
                    .HasMaxLength(50)
                    .HasColumnName("CustomerFName");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.CustomerLname)
                    .HasMaxLength(50)
                    .HasColumnName("CustomerLName");

                entity.Property(e => e.CustomerMobile).HasMaxLength(50);

                entity.Property(e => e.CustomerPostCode).HasMaxLength(50);

                entity.Property(e => e.CustomerStreetName).HasMaxLength(50);

                entity.Property(e => e.CustomerStreetNo).HasMaxLength(50);

                entity.Property(e => e.CustomerSuburb).HasMaxLength(50);

                entity.Property(e => e.PlacementEstimatedHours).HasColumnType("numeric(18, 2)");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<TblProvider>(entity =>
            {
                entity.HasKey(e => e.ProviderId);

                entity.ToTable("tbl_Provider");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.ProviderAbn)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderABN");

                entity.Property(e => e.ProviderCompanyName).HasMaxLength(50);

                entity.Property(e => e.ProviderEmail).HasMaxLength(50);

                entity.Property(e => e.ProviderEmergencyContactName).HasMaxLength(100);

                entity.Property(e => e.ProviderEmergencyContactNumber).HasMaxLength(50);

                entity.Property(e => e.ProviderFname)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderFName");

                entity.Property(e => e.ProviderLname)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderLName");

                entity.Property(e => e.ProviderMobile).HasMaxLength(50);

                entity.Property(e => e.ProviderPostCode).HasMaxLength(4);

                entity.Property(e => e.ProviderSuburb).HasMaxLength(50);

                entity.Property(e => e.ProviderTypeId).HasColumnName("ProviderTypeID");

                entity.Property(e => e.SocialId).HasColumnName("SocialID");

                entity.Property(e => e.VisaTypeId).HasColumnName("VisaTypeID");

                entity.Property(e => e.YearsofCleaningExpId).HasColumnName("YearsofCleaningExpID");
            });

            modelBuilder.Entity<TblProviderArea>(entity =>
            {
                entity.HasKey(e => e.ProviderArea);

                entity.ToTable("tbl_ProviderArea");

                entity.Property(e => e.AreaId).HasColumnName("AreaID");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            });

            modelBuilder.Entity<TblProviderAvailability>(entity =>
            {
                entity.HasKey(e => e.ProviderAvailibilityId);

                entity.ToTable("tbl_ProviderAvailability");

                entity.Property(e => e.ProviderAvailibilityId).HasColumnName("ProviderAvailibilityID");

                entity.Property(e => e.Afternoons125).HasColumnName("Afternoons12-5");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            });

            modelBuilder.Entity<TblProviderEquipments>(entity =>
            {
                entity.HasKey(e => e.ProviderEquipmentId);

                entity.ToTable("tbl_ProviderEquipments");

                entity.Property(e => e.ProviderEquipmentId).HasColumnName("ProviderEquipmentID");

                entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            });

            modelBuilder.Entity<TblProviderServices>(entity =>
            {
                entity.HasKey(e => e.ProviderServiceId);

                entity.ToTable("tbl_ProviderServices");

                entity.Property(e => e.ProviderServiceId).HasColumnName("ProviderServiceID");

                entity.Property(e => e.CleaningTypeId).HasColumnName("CleaningTypeID");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .HasColumnName("CreatedBY");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            });

            modelBuilder.Entity<TblProviderVersionHistory>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tbl_ProviderVersionHistory");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .HasColumnName("ModifiedBY");

                entity.Property(e => e.ModifiedOn).HasColumnName("ModifiedON");

                entity.Property(e => e.ProviderAbn)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderABN");

                entity.Property(e => e.ProviderCompanyName).HasMaxLength(50);

                entity.Property(e => e.ProviderEmail).HasMaxLength(50);

                entity.Property(e => e.ProviderEmergencyContactName).HasMaxLength(100);

                entity.Property(e => e.ProviderEmergencyContactNumber).HasMaxLength(50);

                entity.Property(e => e.ProviderFname)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderFName");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");

                entity.Property(e => e.ProviderLname)
                    .HasMaxLength(50)
                    .HasColumnName("ProviderLName");

                entity.Property(e => e.ProviderMobile).HasMaxLength(50);

                entity.Property(e => e.ProviderPostCode).HasMaxLength(4);

                entity.Property(e => e.ProviderSuburb).HasMaxLength(50);

                entity.Property(e => e.ProviderTypeId).HasColumnName("ProviderTypeID");
            });

            modelBuilder.Entity<TblReviews>(entity =>
            {
                entity.HasKey(e => e.ReviewId);

                entity.ToTable("tbl_Reviews");

                entity.Property(e => e.ReviewId).HasColumnName("ReviewID");

                entity.Property(e => e.CreatedOn).HasColumnName("CreatedON");

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.PlacementId).HasColumnName("PlacementID");

                entity.Property(e => e.ProviderId).HasColumnName("ProviderID");
            });

            modelBuilder.Entity<TblTransactionHistories>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PK__Transact__55433A6B1BC3291A");

                entity.ToTable("tbl_TransactionHistories");

                entity.Property(e => e.AmountPaid).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.DeletedBy).HasMaxLength(256);

                entity.Property(e => e.DeletedOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentMethod).HasMaxLength(128);

                entity.Property(e => e.StripeChargeId).HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
