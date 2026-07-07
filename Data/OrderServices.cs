using Microsoft.EntityFrameworkCore;
using Sylvan.Data.Csv;
using System.Data;

namespace ReachingOutDB.Data
{
    public class OrderServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private AuditLogServices auditLogServices;
        private CustomerServices customerServices;
        #endregion

        #region Constructor
        public OrderServices(IDbContextFactory<AppDbContext> contextFactory, AuditLogServices auditLogServices, CustomerServices customerServices)
        {
            this.contextFactory = contextFactory;
            this.auditLogServices = auditLogServices;
            this.customerServices = customerServices;
        }
        #endregion

        public async Task<ICollection<Order>> GetOrdersAsync(int? year, Quarter? quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            if (year != null &&  quarter != null)
            {
                return await dbContext.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.Year == year)
                    .Where(o => o.Quarter == quarter)
                    .ToListAsync();
            }
            else if (year != null)
            {
                return await dbContext.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.Year == year)
                    .ToListAsync();
            }
            else if (quarter != null)
            {
                return await dbContext.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.Quarter == quarter)
                    .ToListAsync();
            }
            else
            {
                return await dbContext.Orders.Include(o => o.Customer).Include(o => o.PlateAssignments).ToListAsync();
            }
        }

        public async Task<ICollection<Order>> GetOrdersIncludePlateAssignmentsAsync(int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.PlateAssignments)
                    .ThenInclude(pa => pa.Plate)
                    .Where(o => o.Year == year)
                    .Where(o => o.Quarter == quarter)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Orders
                .Include(o => o.Customer)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        // Creates one order per active customer for the given year/quarter, seeded from
        // that customer's per-quarter (or flat) quantities/notes. Orders with 0 qty are skipped.
        public async Task<int> GenerateOrders(int? customerId, int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            List<Customer> customers;
            if (customerId == null)
            {
                customers = await dbContext.Customers.Where(c => c.Active == true).ToListAsync();
            }
            else
            {
                customers = await dbContext.Customers.Where(c => c.CustomerId == customerId).ToListAsync();
            }

            foreach (var customer in customers)
            {
                Order order = new Order();
                order.OrderId = Guid.NewGuid();
                order.Year = year;
                order.Customer = customer;
                order.Quarter = quarter;
                order.JobStatus = JobStatus.ReadyToPlate;

                if (customer.YearlyBillingQuarter != null)
                {
                    order.YearlyBilling = (customer.YearlyBillingQuarter == quarter) ? true : null;
                }
                else
                {
                    order.YearlyBilling = false;
                }
                order.BpUpdate = false;
                order.CustomBP = customer.CustomBP;
                order.Archived = false;
                if (customer.VariableOrders)
                {
                    if (quarter == Quarter.Q1)
                    {
                        order.Qty = (int)customer.QtyQ1;
                        order.SpecialNotes = customer.NotesQ1;
                        order.DmQty = customer.DmQtyQ1;
                        order.UpsQty = customer.UpsQtyQ1;
                        order.PostalQty = customer.PostalQtyQ1;
                        order.LtlQty = customer.LtlQtyQ1;
                        order.IntlQty = customer.IntlQtyQ1;
                    }
                    else if (quarter == Quarter.Q2)
                    {
                        order.Qty = (int)customer.QtyQ2;
                        order.SpecialNotes = customer.NotesQ2;
                        order.DmQty = customer.DmQtyQ2;
                        order.UpsQty = customer.UpsQtyQ2;
                        order.PostalQty = customer.PostalQtyQ2;
                        order.LtlQty = customer.LtlQtyQ2;
                        order.IntlQty = customer.IntlQtyQ2;
                    }
                    else if (quarter == Quarter.Q3)
                    {
                        order.Qty = (int)customer.QtyQ3;
                        order.SpecialNotes = customer.NotesQ3;
                        order.DmQty = customer.DmQtyQ3;
                        order.UpsQty = customer.UpsQtyQ3;
                        order.PostalQty = customer.PostalQtyQ3;
                        order.LtlQty = customer.LtlQtyQ3;
                        order.IntlQty = customer.IntlQtyQ3;
                    }
                    else if (quarter == Quarter.Q4)
                    {
                        order.Qty = (int)customer.QtyQ4;
                        order.SpecialNotes = customer.NotesQ4;
                        order.DmQty = customer.DmQtyQ4;
                        order.UpsQty = customer.UpsQtyQ4;
                        order.PostalQty = customer.PostalQtyQ4;
                        order.LtlQty = customer.LtlQtyQ4;
                        order.IntlQty = customer.IntlQtyQ4;
                    }
                }
                else
                {
                    order.Qty = (int)customer.Qty;
                    order.SpecialNotes = customer.Notes;
                    order.DmQty = customer.DmQty;
                    order.UpsQty = customer.UpsQty;
                    order.PostalQty = customer.PostalQty;
                    order.LtlQty = customer.LtlQty;
                    order.IntlQty = customer.IntlQty;
                }
                if (order.Qty > 0)
                {
                    dbContext.Orders.Add(order);
                }
            }
            //await auditLogServices.LogOrderChangesAsync(dbContext);
            return await dbContext.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order updatedOrder)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            var originalOrder = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == updatedOrder.OrderId);

            if (originalOrder == null)
            {
                throw new InvalidOperationException($"Order with ID {updatedOrder.OrderId} not found.");
            }

            //Calculate quantity
            updatedOrder.Qty = (updatedOrder.UpsQty ?? 0) + (updatedOrder.PostalQty ?? 0) +
                (updatedOrder.LtlQty ?? 0) + (updatedOrder.IntlQty ?? 0) + (updatedOrder.DmQty ?? 0);

            // Check if any shipping cost fields changed
            if (originalOrder.UpsCost != updatedOrder.UpsCost ||
                originalOrder.IntlCost != updatedOrder.IntlCost ||
                originalOrder.LTLCost != updatedOrder.LTLCost)
            {
                // Recalculate shipping (this now only updates the order object, doesn't save)
                updatedOrder = await CalculatePublishedShippingAsync(updatedOrder);
            }

            if (originalOrder.PostalCost != updatedOrder.PostalCost ||
                originalOrder.DmCost != updatedOrder.DmCost)
            {
                updatedOrder = await CalculatePublishedPostageAsync(updatedOrder);
            }

            if (originalOrder.DmQty != updatedOrder.DmQty)
            {
                if (!updatedOrder.Customer.VariableOrders)
                {
                    Customer customer = updatedOrder.Customer;
                    customer.DmQty = updatedOrder.DmQty;
                    await customerServices.UpdateCustomerAsync(customer);
                }
            }
            await auditLogServices.LogOrderChangesAsync(originalOrder, updatedOrder);
            dbContext.Orders.Update(updatedOrder);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(Order order)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            var auditLogs = await dbContext.OrderAuditLogs.Where(l => l.Order == order).ToListAsync();
            if (auditLogs.Any())
            {
                foreach (var log in auditLogs)
                {
                    dbContext.OrderAuditLogs.Remove(log);
                }
            }
            dbContext.Orders.Remove(order);
            await dbContext.SaveChangesAsync();
        }

        // PubUsps is the customer-facing (published) postage price: actual cost plus markup
        // and a per-box fee, boxes being ceil(qty / QuantityPerBox). If a direct-mail (DM) cost
        // is set, that vendor cost is used as-is since it already includes markup.
        public async Task<Order> CalculatePublishedPostageAsync(Order order)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                if (!order.DmCost.HasValue)
                {
                    var uspsShipSettings = await dbContext.ShippingSettings.FirstOrDefaultAsync(s => s.Name == "USPS");
                    int numberOfBoxes = (order.PostalQty.Value + uspsShipSettings.QuantityPerBox - 1) / uspsShipSettings.QuantityPerBox;
                    order.PubUsps = order.PostalCost + (order.PostalCost * uspsShipSettings.MarkupPercentage) + (uspsShipSettings.PerBoxFee * numberOfBoxes);
                }
                else
                {
                    order.PubUsps = order.DmCost;
                }

                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return order;
            }
        }

        // PubShipping sums the published (marked-up) price across whichever of UPS/INTL/LTL
        // costs are set on the order. Each adds its own markup, handling fee, and per-box fee;
        // UPS additionally gets a discount once box count passes BoxDiscountThreshold.
        public async Task<Order> CalculatePublishedShippingAsync(Order order)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            try
            {
                decimal pubShip = 0m;

                if (order.UpsCost.HasValue)
                {
                    var upsShipSettings = await dbContext.ShippingSettings
                        .FirstOrDefaultAsync(s => s.Name == "UPS");
                    int numberOfBoxes = (order.UpsQty.Value + upsShipSettings.QuantityPerBox - 1) / upsShipSettings.QuantityPerBox;
                    pubShip = order.UpsCost.Value +
                             (order.UpsCost.Value * upsShipSettings.MarkupPercentage) +
                             upsShipSettings.HandlingFee +
                             (upsShipSettings.PerBoxFee * numberOfBoxes);

                    if (numberOfBoxes > upsShipSettings.BoxDiscountThreshold.Value)
                    {
                        pubShip -= ((decimal)numberOfBoxes / upsShipSettings.BoxDiscountThreshold.Value - 1) *
                                  upsShipSettings.BoxDiscountPercentage.Value * order.UpsCost.Value;
                    }
                }

                if (order.IntlCost.HasValue)
                {
                    var intlShipSettings = await dbContext.ShippingSettings
                        .FirstOrDefaultAsync(s => s.Name == "INTL");
                    int numberOfBoxes = (order.IntlQty.Value + intlShipSettings.QuantityPerBox - 1) / intlShipSettings.QuantityPerBox;
                    pubShip += order.IntlCost.Value +
                              (order.IntlCost.Value * intlShipSettings.MarkupPercentage) +
                              intlShipSettings.HandlingFee +
                              (intlShipSettings.PerBoxFee * numberOfBoxes);
                }

                if (order.LTLCost.HasValue)
                {
                    var ltlShipSettings = await dbContext.ShippingSettings
                        .FirstOrDefaultAsync(s => s.Name == "LTL");
                    int numberOfBoxes = (order.LtlQty.Value + ltlShipSettings.QuantityPerBox - 1) / ltlShipSettings.QuantityPerBox;
                    pubShip += order.LTLCost.Value +
                              (order.LTLCost.Value * ltlShipSettings.MarkupPercentage) +
                              ltlShipSettings.HandlingFee +
                              (ltlShipSettings.PerBoxFee * numberOfBoxes);
                }

                // Just update the property, don't save
                order.PubShipping = pubShip;
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return order;
            }
        }

        public async Task<string> ImportEndiciaPrintLogCsvAsync(string path, int year, Quarter quarter)
        {
            string successMessage = "";
            string errorMessage = "";
            int successCt = 0;
            int errrorCt = 0;
            try
            {
                var orders = await GetOrdersAsync(year, quarter);
                CsvDataReader reader = CsvDataReader.Create(path, new CsvDataReaderOptions
                {
                    Delimiter = ',',
                    HasHeaders = true
                });
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

                foreach (DataRow row in dataTable.Rows)
                {
                    try
                    {
                        // Endicia's "Order Number" column is actually the customer ID, not an order ID.
                        var customerId = int.Parse(row["Order Number"].ToString());
                        var order = orders.FirstOrDefault(o => o.CustomerId == customerId);
                        order.PostalCost = (order.PostalCost ?? 0) + Convert.ToDecimal(row["Cost"].ToString());
                        await UpdateOrderAsync(order);
                        successMessage += $"Added postage cost for {order.Customer.CustomerName} - {customerId}{Environment.NewLine}";
                        successCt++;
                    }
                    catch
                    {
                        errorMessage += $"Could not add customer with ID: {row["Order Number"].ToString()} Cost: {row["Cost"].ToString()}{Environment.NewLine}";
                        errrorCt++;
                    }
                }
                successMessage = $"There were {successCt.ToString()} records imported{Environment.NewLine}" + successMessage;
                errorMessage = $"There were {errrorCt.ToString()} records that could not be imported{Environment.NewLine}" + errorMessage;
                return successMessage + $"{Environment.NewLine}{Environment.NewLine}" + errorMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.ToString();
            }
        }
    }
}
