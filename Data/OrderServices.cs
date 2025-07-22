using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ReachingOutDB.Data
{
    public class OrderServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private OrderAuditLogServices auditLogServices;
        #endregion

        #region Constructor
        public OrderServices(IDbContextFactory<AppDbContext> contextFactory, OrderAuditLogServices auditLogServices)
        {
            this.contextFactory = contextFactory;
            this.auditLogServices = auditLogServices;
        }
        #endregion

        public async Task<IEnumerable<Order>> GetOrdersAsync(int? year, Quarter? quarter)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
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
                return await dbContext.Orders.Include(o => o.Customer).ToListAsync();
            }
        }

        public async Task<int> GenerateOrders(int? customerId, int year, Quarter quarter)
        {
            try
            {
                var dbContext = await contextFactory.CreateDbContextAsync();
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
                    if (quarter == Quarter.Q1)
                    { 
                        order.Qty = customer.QtyQ1;
                        order.SpecialNotes = customer.NotesQ1;
                    }
                    else if (quarter == Quarter.Q2)
                    {
                        order.Qty = customer.QtyQ2;
                        order.SpecialNotes = customer.NotesQ2;
                    }
                    else if(quarter == Quarter.Q3)
                    {
                        order.Qty = customer.QtyQ3;
                        order.SpecialNotes = customer.NotesQ3;
                    }
                    else if (quarter == Quarter.Q4)
                    {
                        order.Qty = customer.QtyQ4;
                        order.SpecialNotes = customer.NotesQ4;
                    }
                    if (customer.YearlyBillingQuarter != null)
                    {
                        order.YearlyBilling = (customer.YearlyBillingQuarter == quarter);
                    }
                    order.BpUpdate = false;
                    order.Archived = false;
                    if (customer.VariableQty)
                    {
                        if (quarter == Quarter.Q1)
                        {
                            order.DmQty = customer.DmQtyQ1;
                            order.UpsQty = customer.UpsQtyQ1;
                            order.PostalQty = customer.PostalQtyQ1;
                            order.LtlQty = customer.LtlQtyQ1;
                            order.IntlQty = customer.IntlQtyQ1;
                        }
                        else if (quarter == Quarter.Q2)
                        {
                            order.DmQty = customer.DmQtyQ2;
                            order.UpsQty = customer.UpsQtyQ2;
                            order.PostalQty = customer.PostalQtyQ2;
                            order.LtlQty = customer.LtlQtyQ2;
                            order.IntlQty = customer.IntlQtyQ2;
                        }
                        else if (quarter == Quarter.Q3)
                        {
                            order.DmQty = customer.DmQtyQ3;
                            order.UpsQty = customer.UpsQtyQ3;
                            order.PostalQty = customer.PostalQtyQ3;
                            order.LtlQty = customer.LtlQtyQ3;
                            order.IntlQty = customer.IntlQtyQ3;
                        }
                        else if (quarter == Quarter.Q4)
                        {
                            order.DmQty = customer.DmQtyQ4;
                            order.UpsQty = customer.UpsQtyQ4;
                            order.PostalQty = customer.PostalQtyQ4;
                            order.LtlQty = customer.LtlQtyQ4;
                            order.IntlQty = customer.IntlQtyQ4;
                        }
                    }
                    else
                    {
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        public async Task UpdateOrderAsync(Order updatedOrder)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                var oringinalOrder = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == updatedOrder.OrderId);

                if (oringinalOrder == null)
                {
                    throw new InvalidOperationException($"Order with ID {updatedOrder.OrderId} not found.");
                }

                await auditLogServices.LogOrderChangesAsync(oringinalOrder, updatedOrder);
                dbContext.Orders.Update(updatedOrder);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task DeleteOrderAsync(Order order)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task CalculatePublishedPostageAsync(Order order)
        {
            if (!order.DmQty.HasValue &&  order.DmQty.Value < 1)
            {
                var dbContext = await contextFactory.CreateDbContextAsync();
                var uspsShipSettings = await dbContext.ShippingSettings.FirstOrDefaultAsync(s => s.Name == "USPS");
                int numberOfBoxes = (order.PostalQty.Value + uspsShipSettings.QuantityPerBox - 1) / uspsShipSettings.QuantityPerBox;
                order.PubUsps = order.PostalCost + (order.PostalCost * uspsShipSettings.MarkupPercentage) + (uspsShipSettings.PerBoxFee * numberOfBoxes);
                await UpdateOrderAsync(order);
            }
        }

        public async Task CalculatePublishedShippingAsync(Order order)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();

            decimal pubShip = 0m;

            if (order.UpsCost != null)
            {
                var upsShipSettings = await dbContext.ShippingSettings.FirstOrDefaultAsync(s => s.Name == "UPS");
                int numberOfBoxes = (order.UpsQty.Value + upsShipSettings.QuantityPerBox - 1) / upsShipSettings.QuantityPerBox;
                pubShip = order.UpsCost.Value + (order.UpsCost.Value * upsShipSettings.MarkupPercentage) + upsShipSettings.HandlingFee + (upsShipSettings.PerBoxFee * numberOfBoxes);
                if (numberOfBoxes > 4)
                {
                    decimal discount = (numberOfBoxes / 4 - 1) * -0.15m * order.UpsCost.Value;
                    pubShip -= (numberOfBoxes/upsShipSettings.BoxDiscountThreshold.Value - 1) * upsShipSettings.BoxDiscountPercentage.Value * order.UpsCost.Value;
                }
            }
            if (order.IntlCost != null)
            {
                var intlShipSettings = await dbContext.ShippingSettings.FirstOrDefaultAsync(s => s.Name == "INTL");
                int numberOfBoxes = (order.IntlQty.Value + intlShipSettings.QuantityPerBox - 1) / intlShipSettings.QuantityPerBox;
                pubShip += order.IntlCost.Value + (order.IntlCost.Value * intlShipSettings.MarkupPercentage) + intlShipSettings.HandlingFee + (intlShipSettings.PerBoxFee * numberOfBoxes);
            }
            if (order.LTLCost != null)
            {
                var ltlShipSettings = await dbContext.ShippingSettings.FirstOrDefaultAsync(s => s.Name == "LTL");
                int numberOfBoxes = (order.LtlQty.Value + ltlShipSettings.QuantityPerBox - 1) / ltlShipSettings.QuantityPerBox;
                pubShip += order.LTLCost.Value + (order.LTLCost.Value * ltlShipSettings.MarkupPercentage) + ltlShipSettings.HandlingFee + (ltlShipSettings.PerBoxFee * numberOfBoxes);
            }
            order.PubShipping = pubShip;
            await UpdateOrderAsync(order);
        }
    }
}
