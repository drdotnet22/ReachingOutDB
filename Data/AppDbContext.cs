using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) {}

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderAuditLog> OrderAuditLogs { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageOption> PackageOptions { get; set; }
        public DbSet<ShippingSetting> ShippingSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(GetCustomers());
            modelBuilder.Entity<Order>().HasData(GetOrders());
            modelBuilder.Entity<OrderAuditLog>().HasData(GetOrderAuditLogs());
            modelBuilder.Entity<UserProfile>().HasData(GetUserProfiles());
            modelBuilder.Entity<Package>().HasData(GetPackages());
            modelBuilder.Entity<PackageOption>().HasData(GetPackageOptions());
            modelBuilder.Entity<ShippingSetting>().HasData(GetShippingSettings());

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .IsRequired();
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Packages)
                .HasForeignKey(p => p.CustomerId)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .HasOne(p => p.PackageOption)
                .WithOne(p => p.Package)
                .IsRequired();
        }

        private List<Customer> GetCustomers()
        {
            return new List<Customer>
            {
                new Customer { CustomerId = 2000, CustomerName = "Mennonite Church", Location = "PA", Active = true, QtyQ1 = 0, QtyQ2 = 0, QtyQ3 = 0, QtyQ4 = 0, VariableQty = false, CustomBP = false }
            };
        }

        private List<Order> GetOrders()
        {
            return new List<Order>
            {
                new Order { OrderId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), CustomerId = 2000, Year = 2025, Quarter = Quarter.Q3, JobStatus = JobStatus.OnHold, Qty = 0,  BpUpdate = false, Archived = false }
            };
        }

        private List<OrderAuditLog> GetOrderAuditLogs()
        {
            return new List<OrderAuditLog>
            {
                new OrderAuditLog { OrderAuditLogId = new Guid("02bbd91b-1be0-4640-b82f-66b38ba448b9"), Timestamp = new DateTime(2025, 06, 25), UserName = "Anonymous", OrderId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), OldValue = "Old Value", NewValue = "New Value", PropertyName = "Some Property", Action = "Updated" }
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

        private List<PackageOption> GetPackageOptions()
        {
            return new List<PackageOption>
            {
                new PackageOption { PackageOptionId = new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), PackageDescription = "10x13 plastic sleeve", PackagingWeight = 0.1m }
            };
        }

        private List<ShippingSetting> GetShippingSettings()
        {
            return new List<ShippingSetting>
            {
                new ShippingSetting { Id = 1, Name = "UPS", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 4, MarkupPercentage = 0.6m, PerBoxFee = 1.75m, UpdatedAt = new DateTime(2025, 06, 25) },
                new ShippingSetting { Id = 2, Name = "INTL", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 2, MarkupPercentage = 0.3m, PerBoxFee = 1.25m, UpdatedAt = new DateTime(2025, 06, 25) },
                new ShippingSetting { Id = 3, Name = "LTL", QuantityPerBox = 750, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 25, MarkupPercentage = 0.15m, PerBoxFee = 1.25m, UpdatedAt = new DateTime(2025, 06, 25) },
                new ShippingSetting { Id = 4, Name = "USPS", QuantityPerBox = 200, BoxDiscountPercentage = 0.15m, BoxDiscountThreshold = 4, HandlingFee = 0, MarkupPercentage = 0.1m, PerBoxFee = 1.5m, UpdatedAt = new DateTime(2025, 06, 25) }
            };
        }
    }
}
