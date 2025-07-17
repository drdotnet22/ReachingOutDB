using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReachingOutDB.Data
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Location { get; set; }
        public bool Active { get; set; }
        public int QtyQ1 { get; set; }
        public int QtyQ2 { get; set; }
        public int QtyQ3 { get; set; }
        public int QtyQ4 { get; set; }
        public bool VariableQty { get; set; }
        public string? NotesQ1 { get; set; }
        public string? NotesQ2 { get; set; }
        public string? NotesQ3 { get; set; }
        public string? NotesQ4 { get; set; }
        public bool CustomBP { get; set; }
        //DM
        public int? DmQty { get; set; }
        public int? DmQtyQ1 { get; set; }
        public int? DmQtyQ2 { get; set; }
        public int? DmQtyQ3 { get; set; }
        public int? DmQtyQ4 { get; set; }
        //UPS
        public int? UpsQty { get; set; }
        public int? UpsQtyQ1 { get; set; }
        public int? UpsQtyQ2 { get; set; }
        public int? UpsQtyQ3 { get; set; }
        public int? UpsQtyQ4 { get; set; }
        public bool SpecialNoteUPS { get; set; } = false;
        //USPS
        public int? PostalQty { get; set; }
        public int? PostalQtyQ1 { get; set; }
        public int? PostalQtyQ2 { get; set; }
        public int? PostalQtyQ3 { get; set; }
        public int? PostalQtyQ4 { get; set; }
        //LTL
        public int? LtlQty { get; set; }
        public int? LtlQtyQ1 { get; set; }
        public int? LtlQtyQ2 { get; set; }
        public int? LtlQtyQ3 { get; set; }
        public int? LtlQtyQ4 { get; set; }
        //INTL
        public int? IntlQty { get; set; }
        public int? IntlQtyQ1 { get; set; }
        public int? IntlQtyQ2 { get; set; }
        public int? IntlQtyQ3 { get; set; }
        public int? IntlQtyQ4 { get; set; }
        public Quarter? YearlyBillingQuarter { get; set; }
        public Guid? PackageId { get; set; }
        public ICollection<Package> Packages { get; } = new List<Package>();
    }

    public class Order
    {
        public Guid OrderId { get; set; }
        public int Year { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public Quarter Quarter { get; set; }
        public JobStatus JobStatus { get; set; }
        public int Qty { get; set; } = 0;
        public string? SpecialNotes { get; set; }
        public string? NotesForInvoicing { get; set; }
        public bool? YearlyBilling { get; set; }
        public string? HoldNote { get; set; }
        public int? PlateId { get; set; }
        public bool BpUpdate { get; set; }
        //DM
        public int? DmQty { get; set; }
        //UPS
        public int? UpsQty { get; set; }
        public decimal? UpsCost { get; set; }
        //USPS
        public int? PostalQty { get; set; }
        public decimal? PostalCost { get; set; }
        //INTL
        public int? IntlQty { get; set; }
        public decimal? IntlCost { get; set; }
        //LTL
        public int? LtlQty { get; set; }
        public decimal? LTLCost { get; set; }
        //Published prices
        public decimal? PubUsps { get; set; }
        public decimal? PubShipping { get; set; }
        public int? PpOrderNumber { get; set; }
        public bool Archived { get; set; }
    }

    public class OrderAuditLog
    {
        public Guid OrderAuditLogId { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public Order Order { get; set; }
        public Guid OrderId { get; set; }
        public string? PropertyName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Action { get; set; }
    }

    public class UserProfile
    {
        public int UserProfileId { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public bool Active { get; set; }
    }

    public class Package
    {
        public Guid PackageId { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int Qty { get; set; } = 1;
        public string MailClass { get; set; }
        public Guid PackageOptionId { get; set; }
        public PackageOption PackageOption { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    public class PackageOption
    {
        public Guid PackageOptionId { get; set; }
        public string PackageDescription { get; set; }
        public decimal PackagingWeight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }

    public class ShippingSetting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuantityPerBox { get; set; }
        public decimal MarkupPercentage { get; set; } = 0.6m;
        public decimal HandlingFee { get; set; } = 4m;
        public decimal PerBoxFee { get; set; } = 1.75m;
        public int? BoxDiscountThreshold { get; set; } = 4;
        public decimal? BoxDiscountPercentage { get; set; } = 0.15m;
        public DateTime UpdatedAt { get; set; }
    }

    public class MiscSetting
    {
        public int Id { get; set; }
        public decimal MagazineWeight { get; set; }
    }

    public enum Quarter
    {
        Q1 = 1,
        Q2 = 2,
        Q3 = 3,
        Q4 = 4
    }

    public enum JobStatus
    {
        OnHold = 1,
        ReadyToPlate = 2,
        Plated = 3,
        ReadyToShip = 4,
        Shipped = 5,
        Invoiced = 6
    }

    public enum UserRole
    {
        UnAssigned = 0,
        Admin = 1,
        Duplo = 2,
        Shipping = 3
    }
}
