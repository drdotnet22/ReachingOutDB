using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) {}

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderAuditLog> OrderAuditLogs { get; set; }
        public DbSet<CustomerChangesLog> CustomerChangesLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<IntlPackage> IntlPackages { get; set; }
        public DbSet<PackageOption> PackageOptions { get; set; }
        public DbSet<ShippingSetting> ShippingSettings { get; set; }
        public DbSet<MiscSetting> MiscSettings { get; set; }
        public DbSet<Plate> Plates { get; set; }
        public DbSet<PlateAssignment> PlateAssignments { get; set; }
        public DbSet<SmtpSetting> SmtpSettings { get; set; }
        public DbSet<ReminderRule> ReminderRules { get; set; }
        public DbSet<ReminderCondition> ReminderConditions { get; set; }
        public DbSet<ReminderLog> ReminderLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>().HasData(GetUserProfiles());
            modelBuilder.Entity<ShippingSetting>().HasData(GetShippingSettings());
            modelBuilder.Entity<MiscSetting>().HasData(GetMiscSettings());

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired();

            modelBuilder.Entity<Order>() // Configure the Version property for concurrency control
                .Property(o => o.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            modelBuilder.Entity<Customer>() // Configure the Version property for concurrency control
                .Property(c => c.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            modelBuilder.Entity<CustomerChangesLog>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Packages)
                .HasForeignKey(p => p.CustomerId)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .HasOne(p => p.PackageOption)
                .WithMany()
                .IsRequired();

            modelBuilder.Entity<IntlPackage>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .IsRequired();

            modelBuilder.Entity<PlateAssignment>()
                .HasOne(pa => pa.Plate)
                .WithMany(p => p.PlateAssignments)
                .HasForeignKey(pa => pa.PlateId)
                .IsRequired();

            modelBuilder.Entity<PlateAssignment>()
                .HasOne(pa => pa.Order)
                .WithMany(o => o.PlateAssignments)
                .HasForeignKey(pa => pa.OrderId);

            modelBuilder.Entity<ReminderRule>()
                .HasMany(r => r.Conditions)
                .WithOne(c => c.ReminderRule)
                .HasForeignKey(c => c.ReminderRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReminderRule>() // Configure the Version property for concurrency control
                .Property(r => r.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            modelBuilder.Entity<ReminderLog>()
                .HasOne(l => l.ReminderRule)
                .WithMany()
                .HasForeignKey(l => l.ReminderRuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReminderLog>()
                .HasIndex(l => new { l.ReminderRuleId, l.OrderId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }

        private List<UserProfile> GetUserProfiles()
        {
            return new List<UserProfile>
            {
                new UserProfile { UserProfileId = 1, Name = "Ryan Stauffer", Role = UserRole.Admin, Active = true}
            };
        }

        private List<ShippingSetting> GetShippingSettings()
        {
            var seedDate = new DateTime(2025, 06, 25, 0, 0, 0, DateTimeKind.Utc);
            return new List<ShippingSetting>
            {
                new ShippingSetting { Id = 1, Name = "UPS", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 4, MarkupPercentage = 0.6m, PerBoxFee = 1.75m, UpdatedAt = seedDate },
                new ShippingSetting { Id = 2, Name = "INTL", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 2, MarkupPercentage = 0.3m, PerBoxFee = 1.25m, UpdatedAt = seedDate },
                new ShippingSetting { Id = 3, Name = "LTL", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 25, MarkupPercentage = 0.15m, PerBoxFee = 1.25m, UpdatedAt = seedDate },
                new ShippingSetting { Id = 4, Name = "USPS", QuantityPerBox = 200, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 0, MarkupPercentage = 0.1m, PerBoxFee = 1.5m, UpdatedAt = seedDate }
            };
        }

        private List<MiscSetting> GetMiscSettings()
        {
            return new List<MiscSetting>
            { 
                new MiscSetting { Id = 1, MagazineWeight = 0.06m }
            };
        }
    }
}
