using System;
using System.Collections.Generic;

namespace MaxemusAPI.Models1
{
    public partial class AspNetUsers
    {
        public AspNetUsers()
        {
            AspNetUserClaims = new HashSet<AspNetUserClaims>();
            AspNetUserLogins = new HashSet<AspNetUserLogins>();
            AspNetUserTokens = new HashSet<AspNetUserTokens>();
            DistributorOrder = new HashSet<DistributorOrder>();
            Notification = new HashSet<Notification>();
            NotificationSent = new HashSet<NotificationSent>();
            PointDetail = new HashSet<PointDetail>();
            RedeemedProducts = new HashSet<RedeemedProducts>();
            UserOrder = new HashSet<UserOrder>();
            Role = new HashSet<AspNetRoles>();
        }

        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePic { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? DialCode { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceToken { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }

        public virtual CountryMaster? Country { get; set; }
        public virtual StateMaster? State { get; set; }
        public virtual ICollection<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual ICollection<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual ICollection<DistributorOrder> DistributorOrder { get; set; }
        public virtual ICollection<Notification> Notification { get; set; }
        public virtual ICollection<NotificationSent> NotificationSent { get; set; }
        public virtual ICollection<PointDetail> PointDetail { get; set; }
        public virtual ICollection<RedeemedProducts> RedeemedProducts { get; set; }
        public virtual ICollection<UserOrder> UserOrder { get; set; }

        public virtual ICollection<AspNetRoles> Role { get; set; }
    }
}
