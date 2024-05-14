using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MaxemusAPI.Models1
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

        public virtual DbSet<AccessoriesVariants> AccessoriesVariants { get; set; } = null!;
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; } = null!;
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; } = null!;
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; } = null!;
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; } = null!;
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; } = null!;
        public virtual DbSet<AudioVariants> AudioVariants { get; set; } = null!;
        public virtual DbSet<Brand> Brand { get; set; } = null!;
        public virtual DbSet<CameraVariants> CameraVariants { get; set; } = null!;
        public virtual DbSet<CartDetail> CartDetail { get; set; } = null!;
        public virtual DbSet<CertificationVariants> CertificationVariants { get; set; } = null!;
        public virtual DbSet<CityMaster> CityMaster { get; set; } = null!;
        public virtual DbSet<CompanyDetail> CompanyDetail { get; set; } = null!;
        public virtual DbSet<ContactUs> ContactUs { get; set; } = null!;
        public virtual DbSet<CountryMaster> CountryMaster { get; set; } = null!;
        public virtual DbSet<DealerDetail> DealerDetail { get; set; } = null!;
        public virtual DbSet<DealerProduct> DealerProduct { get; set; } = null!;
        public virtual DbSet<DistributorAddress> DistributorAddress { get; set; } = null!;
        public virtual DbSet<DistributorDetail> DistributorDetail { get; set; } = null!;
        public virtual DbSet<DistributorOrder> DistributorOrder { get; set; } = null!;
        public virtual DbSet<DistributorOrderedProduct> DistributorOrderedProduct { get; set; } = null!;
        public virtual DbSet<EnvironmentVariants> EnvironmentVariants { get; set; } = null!;
        public virtual DbSet<GeneralVariants> GeneralVariants { get; set; } = null!;
        public virtual DbSet<InstallationDocumentVariants> InstallationDocumentVariants { get; set; } = null!;
        public virtual DbSet<LensVariants> LensVariants { get; set; } = null!;
        public virtual DbSet<MainCategory> MainCategory { get; set; } = null!;
        public virtual DbSet<NetworkVariants> NetworkVariants { get; set; } = null!;
        public virtual DbSet<Notification> Notification { get; set; } = null!;
        public virtual DbSet<NotificationSent> NotificationSent { get; set; } = null!;
        public virtual DbSet<OderAddress> OderAddress { get; set; } = null!;
        public virtual DbSet<OrderedPoduct> OrderedPoduct { get; set; } = null!;
        public virtual DbSet<OrderedProductQr> OrderedProductQr { get; set; } = null!;
        public virtual DbSet<PointDetail> PointDetail { get; set; } = null!;
        public virtual DbSet<Points> Points { get; set; } = null!;
        public virtual DbSet<PowerVariants> PowerVariants { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
        public virtual DbSet<ProductStock> ProductStock { get; set; } = null!;
        public virtual DbSet<RedeemedProducts> RedeemedProducts { get; set; } = null!;
        public virtual DbSet<ReedemProducts> ReedemProducts { get; set; } = null!;
        public virtual DbSet<RewardProduct> RewardProduct { get; set; } = null!;
        public virtual DbSet<SmartEvent> SmartEvent { get; set; } = null!;
        public virtual DbSet<StateMaster> StateMaster { get; set; } = null!;
        public virtual DbSet<SubCategory> SubCategory { get; set; } = null!;
        public virtual DbSet<UserDetail> UserDetail { get; set; } = null!;
        public virtual DbSet<UserManual> UserManual { get; set; } = null!;
        public virtual DbSet<UserOrder> UserOrder { get; set; } = null!;
        public virtual DbSet<UserOrderedProduct> UserOrderedProduct { get; set; } = null!;
        public virtual DbSet<VideoVariants> VideoVariants { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessoriesVariants>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .HasName("PK__Accessor__B40CC6CD9336DC1D");

                entity.Property(e => e.ProductId).ValueGeneratedNever();

                entity.Property(e => e.AccessoryId).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Product)
                    .WithOne(p => p.AccessoriesVariants)
                    .HasForeignKey<AccessoriesVariants>(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccessoriesVariants_Product");
            });

            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.Property(e => e.RoleId).HasMaxLength(450);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.Property(e => e.City).HasMaxLength(250);

                entity.Property(e => e.CreatedBy).HasMaxLength(450);

                entity.Property(e => e.DeviceType).HasMaxLength(100);

                entity.Property(e => e.DialCode).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.PostalCode).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.AspNetUsers)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_AspNetUsers_CountryMaster");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.AspNetUsers)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_AspNetUsers_StateMaster");

                entity.HasMany(d => d.Role)
                    .WithMany(p => p.User)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRoles",
                        l => l.HasOne<AspNetRoles>().WithMany().HasForeignKey("RoleId"),
                        r => r.HasOne<AspNetUsers>().WithMany().HasForeignKey("UserId"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId");

                            j.ToTable("AspNetUserRoles");
                        });
            });

            modelBuilder.Entity<AudioVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__AudioVar__0EA233844755CD2D");

                entity.Property(e => e.AudioCompression).HasMaxLength(500);

                entity.Property(e => e.BuiltInMic).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.AudioVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AudioVariants_Product");
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.Property(e => e.BrandName).HasMaxLength(250);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<CameraVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId);

                entity.Property(e => e.Appearance).HasMaxLength(500);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EffectivePixels).HasMaxLength(500);

                entity.Property(e => e.ElectronicShutterSpeed).HasMaxLength(500);

                entity.Property(e => e.ImageSensor).HasMaxLength(500);

                entity.Property(e => e.Irdistance)
                    .HasMaxLength(500)
                    .HasColumnName("IRDistance");

                entity.Property(e => e.IrledsNumber)
                    .HasMaxLength(500)
                    .HasColumnName("IRLEDsNumber");

                entity.Property(e => e.IronOffControl)
                    .HasMaxLength(500)
                    .HasColumnName("IROnOffControl");

                entity.Property(e => e.MinIllumination).HasMaxLength(500);

                entity.Property(e => e.PanTiltRotationRange).HasMaxLength(500);

                entity.Property(e => e.Ram)
                    .HasMaxLength(500)
                    .HasColumnName("RAM");

                entity.Property(e => e.Rom)
                    .HasMaxLength(500)
                    .HasColumnName("ROM");

                entity.Property(e => e.ScanningSystem).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CameraVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CameraVariants_Product");
            });

            modelBuilder.Entity<CartDetail>(entity =>
            {
                entity.HasKey(e => e.CartId)
                    .HasName("PK__Cart__51BCD797CAAA79D2");

                entity.Property(e => e.CartId).HasColumnName("CartID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DistributorId).HasColumnName("DistributorID");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.HasOne(d => d.Distributor)
                    .WithMany(p => p.CartDetail)
                    .HasForeignKey(d => d.DistributorId)
                    .HasConstraintName("FK_CartDetail_DistributorDetail");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CartDetail)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartDetail_Product");

                entity.HasOne(d => d.UserDetail)
                    .WithMany(p => p.CartDetail)
                    .HasForeignKey(d => d.UserDetailId)
                    .HasConstraintName("FK_CartDetail_UserDetail");
            });

            modelBuilder.Entity<CertificationVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__Certific__0EA23384FA3F3D5A");

                entity.Property(e => e.Certifications).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CertificationVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CertificationVariants_Product");
            });

            modelBuilder.Entity<CityMaster>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.State)
                    .WithMany(p => p.CityMaster)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CityMaster_CityMaster");
            });

            modelBuilder.Entity<CompanyDetail>(entity =>
            {
                entity.HasKey(e => e.CompanyId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.PostalCode).HasMaxLength(50);

                entity.Property(e => e.RegistrationNumber).HasMaxLength(100);

                entity.Property(e => e.Timing).HasMaxLength(500);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.Property(e => e.WhatsappNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<ContactUs>(entity =>
            {
                entity.HasKey(e => e.ContactId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(500);

                entity.Property(e => e.FirstName).HasMaxLength(500);

                entity.Property(e => e.LastName).HasMaxLength(500);

                entity.Property(e => e.ModifyDate).HasColumnType("datetime");

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Subject).HasMaxLength(500);
            });

            modelBuilder.Entity<CountryMaster>(entity =>
            {
                entity.HasKey(e => e.CountryId);

                entity.Property(e => e.CountryCode).HasMaxLength(5);

                entity.Property(e => e.CountryName).HasMaxLength(50);

                entity.Property(e => e.Timezone).HasMaxLength(200);
            });

            modelBuilder.Entity<DealerDetail>(entity =>
            {
                entity.HasKey(e => e.DealerId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DistributorCode).HasMaxLength(100);

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.Distributor)
                    .WithMany(p => p.DealerDetail)
                    .HasForeignKey(d => d.DistributorId)
                    .HasConstraintName("FK_DealerDetail_DistributorDetail");
            });

            modelBuilder.Entity<DealerProduct>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.HasOne(d => d.Dealer)
                    .WithMany(p => p.DealerProduct)
                    .HasForeignKey(d => d.DealerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DealerProduct_DealerDetail");

                entity.HasOne(d => d.Distributor)
                    .WithMany(p => p.DealerProduct)
                    .HasForeignKey(d => d.DistributorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DealerProduct_DistributorDetail");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DealerProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DealerProduct_Product");

                entity.HasOne(d => d.ProductStock)
                    .WithMany(p => p.DealerProduct)
                    .HasForeignKey(d => d.ProductStockId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DealerProduct_ProductStock");
            });

            modelBuilder.Entity<DistributorAddress>(entity =>
            {
                entity.HasKey(e => e.AddressId)
                    .HasName("PK__Distribu__091C2AFB4B1E36ED");

                entity.Property(e => e.AddressType)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.City).HasMaxLength(250);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.PostalCode).HasMaxLength(50);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.DistributorAddress)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_DistributorAddress_CountryMaster");

                entity.HasOne(d => d.Distributor)
                    .WithMany(p => p.DistributorAddress)
                    .HasForeignKey(d => d.DistributorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DistributorAddress_DistributorDetail");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.DistributorAddress)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_DistributorAddress_StateMaster");
            });

            modelBuilder.Entity<DistributorDetail>(entity =>
            {
                entity.HasKey(e => e.DistributorId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DistributorCode).HasMaxLength(100);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RegistrationNumber).HasMaxLength(100);

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.DistributorDetail)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_DistributorDetail_DistributorAddress");
            });

            modelBuilder.Entity<DistributorOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.CancelledBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderStatus).HasMaxLength(100);

                entity.Property(e => e.PaymentMethod).HasMaxLength(100);

                entity.Property(e => e.PaymentStatus).HasMaxLength(100);

                entity.Property(e => e.TransactionId).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DistributorOrder)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DistributorOrder_AspNetUsers");
            });

            modelBuilder.Entity<DistributorOrderedProduct>(entity =>
            {
                entity.HasKey(e => e.OrderedProductId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TotalMrp).HasColumnName("TotalMRP");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.DistributorOrderedProduct)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DistributorOrderedProduct_DistributorOrder");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DistributorOrderedProduct)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_DistributorOrderedProduct_Product");
            });

            modelBuilder.Entity<EnvironmentVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__Environm__0EA2338486D2E345");

                entity.Property(e => e.OperatingConditions).HasMaxLength(500);

                entity.Property(e => e.Protection).HasMaxLength(500);

                entity.Property(e => e.StorageTemperature).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.EnvironmentVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EnvironmentVariants_Product");
            });

            modelBuilder.Entity<GeneralVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__GeneralV__0EA233846EA8F05D");

                entity.Property(e => e.CasingMetalPlastic).HasMaxLength(500);

                entity.Property(e => e.Dimensions).HasMaxLength(500);

                entity.Property(e => e.GrossWeight).HasMaxLength(500);

                entity.Property(e => e.NetWeight).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.GeneralVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GeneralVariants_Product");
            });

            modelBuilder.Entity<InstallationDocumentVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InstallationDocumentVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Installat__Produ__19AACF41");
            });

            modelBuilder.Entity<LensVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__LensVari__0EA2338479929D6B");

                entity.Property(e => e.CloseFocusDistance)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Doridistance)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DORIDistance");

                entity.Property(e => e.FieldOfView)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FocalLength)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IrisType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LensType)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MaxAperture)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MountType)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MainCategory>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MainCategoryName).HasMaxLength(500);

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<NetworkVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__NetworkV__0EA23384394CB684");

                entity.Property(e => e.Browser).HasMaxLength(500);

                entity.Property(e => e.EdgeStorage).HasMaxLength(500);

                entity.Property(e => e.Interoperability).HasMaxLength(500);

                entity.Property(e => e.ManagementSoftware).HasMaxLength(500);

                entity.Property(e => e.MobilePhone).HasMaxLength(500);

                entity.Property(e => e.Protocol).HasMaxLength(500);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.NetworkVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NetworkVariants_Product");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasMaxLength(450);

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.NotificationType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(500);

                entity.Property(e => e.UserRole)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Notification)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK_Notification_UserDetail");
            });

            modelBuilder.Entity<NotificationSent>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.NotificationType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(500);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.NotificationSent)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NotificationSent_UserDetail");
            });

            modelBuilder.Entity<OderAddress>(entity =>
            {
                entity.HasKey(e => e.AddressId);

                entity.Property(e => e.AddressType)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.PostalCode).HasMaxLength(50);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.OderAddress)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OderAddress_CountryMaster");

                entity.HasOne(d => d.Distributor)
                    .WithMany(p => p.OderAddress)
                    .HasForeignKey(d => d.DistributorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OderAddress_DistributorDetail");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.OderAddress)
                    .HasForeignKey(d => d.StateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OderAddress_StateMaster");
            });

            modelBuilder.Entity<OrderedPoduct>(entity =>
            {
                entity.HasKey(e => e.OrderedProductId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TotalMrp).HasColumnName("TotalMRP");
            });

            modelBuilder.Entity<OrderedProductQr>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("OrderedProductQR");

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_OrderedProductQR_Product");

                entity.HasOne(d => d.ProductStock)
                    .WithMany()
                    .HasForeignKey(d => d.ProductStockId)
                    .HasConstraintName("FK_OrderedProductQR_ProductStock");
            });

            modelBuilder.Entity<PointDetail>(entity =>
            {
                entity.HasKey(e => e.PointId);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.RedeemedPoints).HasDefaultValueSql("((0))");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PointDetail)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PointDetail_AspNetUsers");
            });

            modelBuilder.Entity<Points>(entity =>
            {
                entity.HasKey(e => e.PointId)
                    .HasName("PK_Point");

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RedeemedPoints).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            modelBuilder.Entity<PowerVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__PowerVar__0EA23384C9D39BF1");

                entity.Property(e => e.PowerConsumption)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PowerSupply)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DistributorDiscount).HasDefaultValueSql("((0))");

                entity.Property(e => e.DistributorSellingPrice).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.Model).HasMaxLength(500);

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TotalMrp).HasColumnName("TotalMRP");

                entity.HasOne(d => d.Brand)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.BrandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Brand");

                entity.HasOne(d => d.MainCategory)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.MainCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_MainCategory");

                entity.HasOne(d => d.SubCategory)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.SubCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_SubCategory");
            });

            modelBuilder.Entity<ProductStock>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Qrcode).HasColumnName("QRCode");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductStock)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductStock_ProductStock");
            });

            modelBuilder.Entity<RedeemedProducts>(entity =>
            {
                entity.HasKey(e => e.ReedemProductId);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.RewardProduct)
                    .WithMany(p => p.RedeemedProducts)
                    .HasForeignKey(d => d.RewardProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RedeemedProducts_RwardProduct");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RedeemedProducts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_RedeemedProducts_AspNetUsers");
            });

            modelBuilder.Entity<ReedemProducts>(entity =>
            {
                entity.HasKey(e => e.ReedemProductId);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.RewardProduct)
                    .WithMany(p => p.ReedemProducts)
                    .HasForeignKey(d => d.RewardProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReedemProducts_RewardProduct");
            });

            modelBuilder.Entity<RewardProduct>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExpiryDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Mrp).HasColumnName("MRP");

                entity.Property(e => e.Stock).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<SmartEvent>(entity =>
            {
                entity.HasKey(e => e.VariantId)
                    .HasName("PK__SmartEve__0EA2338408DBD89D");

                entity.Property(e => e.VariantId).ValueGeneratedNever();

                entity.Property(e => e.GeneralIvsanalytics)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("GeneralIVSAnalytics");
            });

            modelBuilder.Entity<StateMaster>(entity =>
            {
                entity.HasKey(e => e.StateId);

                entity.Property(e => e.StateName).HasMaxLength(50);
            });

            modelBuilder.Entity<SubCategory>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SubCategoryName).HasMaxLength(500);

                entity.HasOne(d => d.MainCategory)
                    .WithMany(p => p.SubCategory)
                    .HasForeignKey(d => d.MainCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubCategory_MainCategory");
            });

            modelBuilder.Entity<UserDetail>(entity =>
            {
                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            modelBuilder.Entity<UserManual>(entity =>
            {
                entity.HasKey(e => e.MannualId);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.UserManual)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__UserManua__Produ__6442E2C9");
            });

            modelBuilder.Entity<UserOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.CancelledBy).HasMaxLength(50);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OrderStatus).HasMaxLength(100);

                entity.Property(e => e.PaymentMethod).HasMaxLength(100);

                entity.Property(e => e.PaymentStatus).HasMaxLength(100);

                entity.Property(e => e.TransactionId).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserOrder)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrder_AspNetUsers");
            });

            modelBuilder.Entity<UserOrderedProduct>(entity =>
            {
                entity.HasKey(e => e.OrderedProductId);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TotalMrp).HasColumnName("TotalMRP");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.UserOrderedProduct)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserOrderedProduct_UserOrder");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.UserOrderedProduct)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_UserOrderedProduct_Product");
            });

            modelBuilder.Entity<VideoVariants>(entity =>
            {
                entity.HasKey(e => e.VariantId);

                entity.Property(e => e.BitRateControl)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Blc)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("BLC");

                entity.Property(e => e.Compression)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DayNight)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.GainControl)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Hlc)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("HLC");

                entity.Property(e => e.ImageRotation)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mirror)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MotionDetection)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NoiseReduction)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PrivacyMasking)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RegionOfInterest)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Resolution)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SmartCodec)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SmartIr)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SmartIR");

                entity.Property(e => e.StreamCapability)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VideoBitRate)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VideoFrameRate)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Wdr)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("WDR");

                entity.Property(e => e.WhiteBalance)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
