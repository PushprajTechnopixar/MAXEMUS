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
        public virtual DbSet<CityMaster> CityMaster { get; set; } = null!;
        public virtual DbSet<CountryMaster> CountryMaster { get; set; } = null!;
        public virtual DbSet<StateMaster> StateMaster { get; set; } = null!;
        public virtual DbSet<NotificationSent> NotificationSent { get; set; } = null!;
        public virtual DbSet<Notification> Notification { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

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

            modelBuilder.Entity<StateMaster>(entity =>
            {
                entity.HasKey(e => e.StateId);

                entity.Property(e => e.StateName).HasMaxLength(50);
            });

            base.OnModelCreating(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }

}
