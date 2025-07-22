using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor.Data;

namespace ReachingOutDB.Data
{
    public class PlateServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public PlateServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
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
        
        public async Task AssignJobsToPlatesAsync(IEnumerable<Order> orders)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                var sortedOrders = orders.OrderByDescending(o => o.Qty).ToList();
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;
                int year = orders.FirstOrDefault().Year;
                Quarter quarter = orders.FirstOrDefault().Quarter;

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 950).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 1500;
                int blankCount = 0;


                while (sortedOrders[0].Qty > 950)
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

                    // Add blanks if needed and order is 10% more than 4th order
                    else if (blankCount < totalBlanksNeeded && sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 3].Qty * 1.1))
                    {
                        //If 3rd job is more than 20% or 400 more than first job, do two blank sheets
                        if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 2].Qty * 1.2) || sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 2].Qty + 400))
                        {
                            plate.Quantity = sortedOrders[orderIndex].Qty;
                            blankCount += sortedOrders[orderIndex].Qty * 2;
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
                            blankCount += sortedOrders[orderIndex].Qty;
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
                    //If blanks are still needed AND order is 4% more than 4th order AND quantity of order is less than 3,000
                    else if (blankCount < totalBlanksNeeded && sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 3].Qty * 1.04) && sortedOrders[orderIndex].Qty < 3000)
                    {
                        plate.Quantity = sortedOrders[orderIndex].Qty;
                        blankCount += sortedOrders[orderIndex].Qty;
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
    }
}