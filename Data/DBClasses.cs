using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        public int? Qty { get; set; }
        public int? QtyQ1 { get; set; }
        public int? QtyQ2 { get; set; }
        public int? QtyQ3 { get; set; }
        public int? QtyQ4 { get; set; }
        public bool VariableOrders { get; set; }
        public string? Notes { get; set; }
        public string? NotesQ1 { get; set; }
        public string? NotesQ2 { get; set; }
        public string? NotesQ3 { get; set; }
        public string? NotesQ4 { get; set; }
        public bool CustomBP { get; set; }
        public Quarter? YearlyBillingQuarter { get; set; }
        public string? MailingNotes {  get; set; }
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
        public int Qty { get; set; }
        public string? SpecialNotes { get; set; }
        public string? NotesForInvoicing { get; set; }
        public bool? YearlyBilling { get; set; }
        public string? HoldNote { get; set; }
        public int? PlateId { get; set; }
        public bool BpUpdate { get; set; }
        public bool CustomBP {  get; set; }
        //DM
        public int? DmQty { get; set; }
        public decimal? DmCost { get; set; }
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
        public ICollection<PlateAssignment> PlateAssignments { get; } = new List<PlateAssignment>();
    }

    public class OrderAuditLog
    {
        public Guid OrderAuditLogId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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

    #region Shipping stuff
    public class Package
    {
        public Guid PackageId { get; set; } = Guid.NewGuid();
        public string? ContactName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MinLength(4, ErrorMessage = "Address is too short")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "A quantity is required")]
        public int Qty { get; set; } = 1;

        [Required(ErrorMessage = "MailClass is required")]
        public string MailClass { get; set; }

        [RequiredNonEmptyGuid(ErrorMessage = "Please select a package option")]
        public Guid PackageOptionId { get; set; }
        public PackageOption PackageOption { get; set; }

        [Range(2000, 2999, ErrorMessage = "Please select a customer")]
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
    #endregion
    #region Plates
    public class Plate
    {
        public Guid PlateId { get; set; }
        public int Number {  get; set; }
        public int Year { get; set; }
        public Quarter Quarter { get; set; }
        public int Quantity { get; set; }
        public bool HasBlanks { get; set; }
        public bool IsPlated { get; set; } = false;
        public ICollection<PlateAssignment> PlateAssignments { get; } = new List<PlateAssignment>();

    }

    public class PlateAssignment
    {
        public Guid PlateAssignmentId { get; set; }
        public Guid PlateId { get; set; }
        public Plate Plate { get; set; }
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public int Position { get; set; }
        public bool IsBlank {  get; set; } = false;
    }
    #endregion

    #region Enums
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
    #endregion

    public class RequiredNonEmptyGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is Guid guid)
            {
                return guid != Guid.Empty;
            }
            return false;
        }
    }
}
