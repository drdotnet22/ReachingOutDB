using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor.Data;

namespace ReachingOutDB.Data
{
    public class PlateServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private OrderServices orderService;
        #endregion

        #region Constructor
        public PlateServices(IDbContextFactory<AppDbContext> contextFactory, OrderServices orderService)
        {
            this.contextFactory = contextFactory;
            this.orderService = orderService;
        }
        #endregion

        public async Task<IEnumerable<Plate>> GetFilteredPlatesAsync(int year, Quarter quarter)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                return await dbContext.Plates
                    .Where(p => p.Year == year)
                    .Where(p => p.Quarter == quarter)
                    .Include(p => p.PlateAssignments)
                    .ThenInclude(p => p.Order)
                    .ThenInclude(o => o.Customer)
                    .OrderBy(p => p.Number)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Enumerable.Empty<Plate>();
            }
        }
        
        public async Task MarkPlatedAsync(Plate plate)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                plate.IsPlated = true;
                foreach (var assignment in plate.PlateAssignments)
                {
                    if (assignment.OrderId != null)
                    {
                        var order = assignment.Order;
                        order.JobStatus = JobStatus.Plated;
                        await orderService.UpdateOrderAsync(order);
                    }
                }
                dbContext.Update(plate);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() );
                throw ex;
            }
        }
        
        public async Task AssignJobsToPlatesAsync(IEnumerable<Order> orders)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;
                int year = orders.FirstOrDefault().Year;
                Quarter quarter = orders.FirstOrDefault().Quarter;

                var sortedOrders = dbContext.Orders
                    .Where(o => o.Year == year)
                    .Where(o => o.Quarter == quarter)
                    .Where(o => ((int)o.JobStatus) < 3)
                    .Where(o => !o.PlateAssignments.Any())
                    .Select(o => new
                    {
                        o.OrderId,
                        o.Qty
                    })
                    .OrderByDescending(o => o.Qty)
                    .ToList();

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 950).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 2000;


                while (sortedOrders[0].Qty > 999)
                {
                    var plate = new Plate()
                    {
                        Year = year,
                        Quarter = quarter,
                        Number = plateNumber++
                    };

                    int platePosition = 0;
                    int orderIndex = 0;
                    sortedOrders.OrderByDescending(o => o.Qty);
                    // If largest quantity is more than 50% more or 1,000 sheets more than 2nd order
                    if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 1].Qty * 1.5) || sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 1].Qty + 1000))
                    {
                        plate.Quantity = sortedOrders[orderIndex].Qty / 2;
                        var targetQty = sortedOrders[orderIndex].Qty / 2;
                        var plateAssignment1 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 1,
                            OrderId = sortedOrders[orderIndex].OrderId,
                            IsBlank = false
                        };
                        plateAssignments.Add(plateAssignment1);

                        var plateAssignment2 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 2,
                            OrderId = sortedOrders[orderIndex].OrderId,
                            IsBlank = false
                        };
                        sortedOrders.Remove(sortedOrders[orderIndex]);
                        plateAssignments.Add(plateAssignment2);

                        var candidates = sortedOrders.Where(o => o.Qty <= targetQty).OrderByDescending(o => o.Qty).Take(2).ToList();
                        foreach (var order in candidates)
                        {
                            var plateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = 3 + candidates.IndexOf(order),
                                OrderId = order.OrderId,
                                IsBlank = false
                            };
                            sortedOrders.Remove(order);
                            plateAssignments.Add(plateAssignment);
                        }
                    }

                    // Add blanks if order is 10% more than 4th order and blanks needed isn't more than 500 more than quantity of order
                    else if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 3].Qty + 600) && sortedOrders[orderIndex].Qty < totalBlanksNeeded + 500)
                    {
                        //If 3rd job is more than 800 more than first job and blanks needed isn't more than 1,000 more than quantity of order, do two blank sheets
                        if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 2].Qty + 500) && sortedOrders[orderIndex].Qty < totalBlanksNeeded + 1000)
                        {
                            plate.Quantity = sortedOrders[orderIndex].Qty;
                            totalBlanksNeeded -= sortedOrders[orderIndex].Qty * 2;
                            var candidates = sortedOrders.Take(2).ToList();
                            foreach (var order in candidates)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    OrderId = order.OrderId,
                                    IsBlank = false
                                };
                                sortedOrders.Remove(order);
                                plateAssignments.Add(plateAssignment);
                            }
                            for (var i = 3; i <= 4; i++)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = i,
                                    IsBlank = true
                                };
                                plateAssignments.Add(plateAssignment);
                            }
                        }
                        else // one blank
                        {
                            plate.Quantity = sortedOrders[orderIndex].Qty;
                            totalBlanksNeeded -= sortedOrders[orderIndex].Qty;
                            var candidates = sortedOrders.Take(3).ToList();
                            foreach (var order in candidates)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    OrderId = order.OrderId,
                                    IsBlank = false
                                };
                                sortedOrders.Remove(order);
                                plateAssignments.Add(plateAssignment);
                            }
                            var blankPlateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = 4,
                                IsBlank = true
                            };
                            plateAssignments.Add(blankPlateAssignment);
                        }
                    }
                    //If blanks are still needed AND order is 5% more than 4th order AND quantity of order is less than 3,000
                    else if (sortedOrders[orderIndex].Qty < totalBlanksNeeded && sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 3].Qty * 1.05) && sortedOrders[orderIndex].Qty < 3000)
                    {
                        plate.Quantity = sortedOrders[orderIndex].Qty;
                        totalBlanksNeeded -= sortedOrders[orderIndex].Qty;
                        var candidates = sortedOrders.Take(3).ToList();
                        foreach (var order in candidates)
                        {
                            var plateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = 1 + candidates.IndexOf(order),
                                OrderId = order.OrderId,
                                IsBlank = false
                            };
                            sortedOrders.Remove(order);
                            plateAssignments.Add(plateAssignment);
                        }
                        var blankPlateAssignment = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 4,
                            IsBlank = true
                        };
                        plateAssignments.Add(blankPlateAssignment);
                    }
                    else //just add 4 jobs to plate
                    {
                        plate.Quantity = sortedOrders[orderIndex].Qty;
                        var candidates = sortedOrders.Take(4).ToList();
                        foreach (var order in candidates)
                        {
                            var plateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = 1 + candidates.IndexOf(order),
                                OrderId = order.OrderId,
                                IsBlank = false
                            };
                            plateAssignments.Add(plateAssignment);
                            sortedOrders.Remove(sortedOrders[orderIndex]);
                        }
                    }
                    dbContext.Add(plate);
                }
                dbContext.AddRange(plateAssignments);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task DeletePlateAsync(Plate plate)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                foreach (var assignment in plate.PlateAssignments)
                {
                    if (assignment.Order?.JobStatus == JobStatus.Plated)
                    {
                        assignment.Order.JobStatus = JobStatus.ReadyToPlate;
                        await orderService.UpdateOrderAsync(assignment.Order);
                    }
                    dbContext.Remove(assignment);
                }
                dbContext.Remove(plate);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}