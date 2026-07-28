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
            // Single seed row per table below is only used to give each migration
            // deterministic starting data; it isn't representative production data.
            //modelBuilder.Entity<Customer>().HasData(GetCustomers());
            //modelBuilder.Entity<Order>().HasData(GetOrders());
            //modelBuilder.Entity<OrderAuditLog>().HasData(GetOrderAuditLogs());
            //modelBuilder.Entity<UserProfile>().HasData(GetUserProfiles());
            //modelBuilder.Entity<Package>().HasData(GetPackages());
            //modelBuilder.Entity<IntlPackage>().HasData(GetIntlPackages());
            //modelBuilder.Entity<PackageOption>().HasData(GetPackageOptions());
            //modelBuilder.Entity<ShippingSetting>().HasData(GetShippingSettings());
            //modelBuilder.Entity<MiscSetting>().HasData(GetMiscSettings());
            //modelBuilder.Entity<Plate>().HasData(GetPlates());
            //modelBuilder.Entity<PlateAssignment>().HasData(GetPlateAssignments());

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

        private List<Customer> GetCustomers()
        {
            return new List<Customer>
            {
                new Customer { CustomerId = 2000, CustomerName = "Mennonite Church", Location = "PA", Active = true, QtyQ1 = 0, QtyQ2 = 0, QtyQ3 = 0, QtyQ4 = 0, VariableOrders = false, CustomBP = false, MailingNotes = "test" }
            };
        }

        private List<Order> GetOrders()
        {
            return new List<Order>
            {
                new Order { OrderId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), CustomerId = 2000, Year = 2025, Quarter = Quarter.Q3, JobStatus = JobStatus.OnHold, Qty = 0,  BpUpdate = false, Archived = false, CustomBP = false }
            };
        }

        private List<OrderAuditLog> GetOrderAuditLogs()
        {
            var seedDate = new DateTime(2025, 06, 25, 0, 0, 0, DateTimeKind.Utc);
            return new List<OrderAuditLog>
            {
                new OrderAuditLog { OrderAuditLogId = new Guid("02bbd91b-1be0-4640-b82f-66b38ba448b9"), Timestamp = seedDate, UserName = "Anonymous", OrderId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), OldValue = "Old Value", NewValue = "New Value", PropertyName = "Some Property", Action = "Updated" }
            };
        }

        private List<UserProfile> GetUserProfiles()
        {
            return new List<UserProfile>
            {
                new UserProfile { UserProfileId = 1, Name = "Ryan Stauffer", Role = UserRole.Admin, Active = true}
            };
        }

        private List<Package> GetPackages()
        {
            return new List<Package>
            {
                new Package { PackageId = new Guid("07dd94e4-a0c8-43c8-babc-200a8864d02c"), ContactName = "Contact name", Address = "123 Main", City = "Ripley", State = "NY", ZipCode = "14775", MailClass = "FCF", PackageOptionId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), CustomerId = 2000 }
            };
        }

        private List<IntlPackage> GetIntlPackages()
        {
            return new List<IntlPackage>
            {
                new IntlPackage { IntlPackageId = new Guid("1c2e6b8a-4f5d-4a3b-9c1e-2d7f8a9b0c3d"), Company = "Mennonite Church", ContactName = "Contact name", Qty = 1, BoxNote = "Box 1 of 1", Address1 = "123 Main", City = "Ripley", State = "NY", ZipCode = "14775", Country = "Canada", CustomerId = 2777 }
            };
        }

        private List<PackageOption> GetPackageOptions()
        {
            return new List<PackageOption>
            {
                new PackageOption { PackageOptionId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), PackageDescription = "10x13 plastic sleeve", PackagingWeight = 0.1m }
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

        private List<Plate> GetPlates()
        {
            return new List<Plate>
            {
                new Plate { PlateId = new Guid("6447999c-271d-4985-6275-08ddc619be12"), Number = 1, Quantity = 1, HasBlanks = false, Year = 1, Quarter = Quarter.Q1 }
            };
        }

        private List<PlateAssignment> GetPlateAssignments()
        {
            return new List<PlateAssignment>
            {
                new PlateAssignment { PlateAssignmentId = new Guid("5ad5f77b-d3cc-4454-acbe-a5cbbc7f158e"), IsBlank = false, OrderId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), PlateId = new Guid("6447999c-271d-4985-6275-08ddc619be12"), Position = 1 }
            };
        }
    }
}
