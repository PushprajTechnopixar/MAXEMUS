using MaxemusAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MaxemusAPI.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(300);
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<Brand> Brand { get; set; } = null!;
        public virtual DbSet<CityMaster> CityMaster { get; set; } = null!;
        public virtual DbSet<CountryMaster> CountryMaster { get; set; } = null!;
        public virtual DbSet<DealerDetail> DealerDetail { get; set; } = null!;
        public virtual DbSet<DistributorDetail> DistributorDetail { get; set; } = null!;
        public virtual DbSet<MainCategory> MainCategory { get; set; } = null!;
        public virtual DbSet<Notification> Notification { get; set; } = null!;
        public virtual DbSet<NotificationSent> NotificationSent { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
        public virtual DbSet<StateMaster> StateMaster { get; set; } = null!;
        public virtual DbSet<SubCategory> SubCategory { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.Entity<Brand>(entity =>
            {
                entity.Property(e => e.BrandName).HasMaxLength(250);

                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");
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

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);
            });

            modelBuilder.Entity<DistributorDetail>(entity =>
            {
                entity.HasKey(e => e.DistributorId);

                entity.Property(e => e.AddressLatitude)
                    .HasMaxLength(250)
                    .HasDefaultValueSql("((30.696561151242133))");

                entity.Property(e => e.AddressLongitude)
                    .HasMaxLength(250)
                    .HasDefaultValueSql("((76.7870075375031))");

                entity.Property(e => e.City)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Landmark).HasMaxLength(500);

                entity.Property(e => e.ModifyDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PinCode).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasMaxLength(450);
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

                entity.Property(e => e.Title).HasMaxLength(500);

                entity.Property(e => e.NotificationType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserRole)
                .HasMaxLength(100)
                .IsUnicode(false);
            });

            modelBuilder.Entity<MainCategory>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.MainCategoryName).HasMaxLength(500);

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");
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

                // entity.HasOne(d => d.User)
                //     .WithMany(p => p.NotificationSent)
                //     .HasForeignKey(d => d.UserId)
                //     .OnDelete(DeleteBehavior.ClientSetNull)
                //     .HasConstraintName("FK_NotificationSent_UserDetail");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Model).HasMaxLength(500);

                entity.Property(e => e.ModifyDate).HasDefaultValueSql("(getdate())");
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





            base.OnModelCreating(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }

}
