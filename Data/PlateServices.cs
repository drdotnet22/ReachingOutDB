using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            int plateNumber = 0;
            int year = orders.FirstOrDefault().Year;
            Quarter quarter = orders.FirstOrDefault().Quarter;

            var sortedOrders = orders
                .Where(o => ((int)o.JobStatus) < 3)
                .Where(o => !o.PlateAssignments.Any())
                .OrderByDescending(o => o.Qty)
                .ToList();
            List<List<PlateAssignment>> plateAssignmentLists = new();

            for (var i = 1; i < 800; i++)
            {
                plateAssignmentLists.Add(await AssignJobsToPlatesHelperRandomAsync(sortedOrders.ToList(), year, quarter));
            }

            // Find the minimum number of distinct plates if plate assignment isn't null
            int minPlateCount = plateAssignmentLists
                .Where(list => list != null)
                .Min(list => list.Select(pa => pa.Plate).Distinct().Count());

            // Step 2: Get all lists with that minimum count, then pick the one with lowest total quantity
            List<PlateAssignment> bestAssignment = plateAssignmentLists
                .Where(list => list != null && list.Select(pa => pa.Plate).Distinct().Count() <= minPlateCount)
                .OrderBy(list => list.Sum(pa => pa.Plate.Quantity))
                .First();

            dbContext.AddRange(bestAssignment.Select(pa => pa.Plate).Distinct());
            dbContext.AddRange(bestAssignment);
            await dbContext.SaveChangesAsync();
        }

        private async Task<List<PlateAssignment>> AssignJobsToPlatesHelperAsync(IList<Order> sortedOrders, int year, Quarter quarter, int threshold)
        {
            try
            {
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 950).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 2000;


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
                    // If largest quantity is more than 50% more or 1000 more than 2nd order
                    if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 1].Qty * 1.5) || sortedOrders[orderIndex].Qty > sortedOrders[orderIndex + 1].Qty + 1000)
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
                            if (targetQty < (order.Qty + (threshold * 1.5)))
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = candidates.IndexOf(order) + 3,
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                            }
                        }
                    }

                    // Add blanks if order is more than <threshold> more than 4th order and blanks needed isn't more than 500 more than quantity of order
                    else if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 3].Qty + threshold) && sortedOrders[orderIndex].Qty < totalBlanksNeeded + 500)
                    {
                        //If 2nd job is more than <threshold> more than first job and blanks needed isn't more than 2,000 more than quantity of order x3, do three blank sheets
                        if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 1].Qty + threshold) && sortedOrders[orderIndex].Qty * 3 < totalBlanksNeeded + 2000)
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
                        //If 3rd job is more than <threshold> more than first job and blanks needed isn't more than 1,500 more than quantity of order x2, do two blank sheets
                        else if (sortedOrders[orderIndex].Qty > (sortedOrders[orderIndex + 2].Qty + threshold) && sortedOrders[orderIndex].Qty * 2 < totalBlanksNeeded + 1500)
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
                }
                return plateAssignments;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private async Task<List<PlateAssignment>> AssignJobsToPlatesHelperTestAsync(IList<Order> sortedOrders, int year, Quarter quarter, int threshold)
        {
            try
            {
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 980).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 1000;

                while (sortedOrders[0].Qty > 980)
                {
                    var plate = new Plate()
                    {
                        Year = year,
                        Quarter = quarter,
                        Number = plateNumber++
                    };

                    int platePosition = 0;
                    // If largest quantity is <threshold> x2 more than 2nd order: do first job 2 up
                    if (sortedOrders[0].Qty > sortedOrders[1].Qty + threshold * 2)
                    {
                        plate.Quantity = sortedOrders[0].Qty / 2;
                        var plateAssignment1 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 1,
                            OrderId = sortedOrders[0].OrderId,
                            IsBlank = false
                        };
                        plateAssignments.Add(plateAssignment1);

                        var plateAssignment2 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 2,
                            OrderId = sortedOrders[0].OrderId,
                            IsBlank = false
                        };
                        sortedOrders.Remove(sortedOrders[0]);
                        plateAssignments.Add(plateAssignment2);

                        var candidates = sortedOrders.Where(o => o.Qty <= plate.Quantity).Take(2).ToList();
                        foreach (var order in candidates)
                        {
                            if (order.Qty < plate.Quantity - threshold)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = candidates.IndexOf(order) + 3,
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed but not urgent
                    else if (sortedOrders[0].Qty > 4000 && sortedOrders[0].Qty < totalBlanksNeeded + 2500)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = sortedOrders.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold || 
                                remaingOrdersQty > totalBlanksNeeded + 1000)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and more urgent
                    else if (sortedOrders[0].Qty > 2500 && totalBlanksNeeded > 0)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = sortedOrders.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold ||
                                remaingOrdersQty > totalBlanksNeeded + 1500)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 2 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and very urgent
                    else if (totalBlanksNeeded > 0)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        foreach (var order in candidates)
                        {
                            if (order.Qty > plate.Quantity - (threshold / 2) ||
                                totalBlanksNeeded < 1)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 2 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }
                    else //just add 4 jobs to plate
                    {
                        plate.Quantity = sortedOrders[0].Qty;
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
                            sortedOrders.Remove(order);
                        }
                    }
                }
                
                if (totalBlanksNeeded > 200)
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {totalBlanksNeeded}");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {totalBlanksNeeded}, Press sheets used: {plateAssignments.Select(pa => pa.Plate).Distinct().Sum(p => p.Quantity)}");
                    return plateAssignments;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private async Task<List<PlateAssignment>> AssignJobsToPlatesHelperRandomAsync(IList<Order> sortedOrders, int year, Quarter quarter)
        {
            Random random = new();
            try
            {
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 980).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 1000;
                int threshold = random.Next(4, 20) * 50;

                while (sortedOrders[0].Qty > 980)
                {
                    var plate = new Plate()
                    {
                        Year = year,
                        Quarter = quarter,
                        Number = plateNumber++
                    };

                    int platePosition = 0;
                    // If largest quantity is <threshold> x2 more than 2nd order: do first job 2 up
                    if (sortedOrders[0].Qty > sortedOrders[1].Qty + threshold * 2)
                    {
                        plate.Quantity = sortedOrders[0].Qty / 2;
                        var plateAssignment1 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 1,
                            OrderId = sortedOrders[0].OrderId,
                            IsBlank = false
                        };
                        plateAssignments.Add(plateAssignment1);

                        var plateAssignment2 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 2,
                            OrderId = sortedOrders[0].OrderId,
                            IsBlank = false
                        };
                        sortedOrders.Remove(sortedOrders[0]);
                        plateAssignments.Add(plateAssignment2);

                        var candidates = sortedOrders.Where(o => o.Qty <= plate.Quantity).Take(2).ToList();
                        threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            if (order.Qty > plate.Quantity - threshold)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = candidates.IndexOf(order) + 3,
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed but not urgent
                    else if (sortedOrders[0].Qty > 4000 && sortedOrders[0].Qty < totalBlanksNeeded + 2500)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = sortedOrders.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold ||
                                remaingOrdersQty > totalBlanksNeeded + 1000)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and more urgent
                    else if (sortedOrders[0].Qty > 2500 && totalBlanksNeeded > 0)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = sortedOrders.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold ||
                                remaingOrdersQty > totalBlanksNeeded + 1500)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 2 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and very urgent
                    else if (totalBlanksNeeded > 0)
                    {
                        plate.Quantity = sortedOrders[0].Qty;
                        var candidates = sortedOrders.Take(4).ToList();

                        threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            if (order.Qty > plate.Quantity - (threshold / 2) ||
                                totalBlanksNeeded < 1)
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
                            else
                            {
                                var blankPlateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 2 + candidates.IndexOf(order),
                                    IsBlank = true
                                };
                                plateAssignments.Add(blankPlateAssignment);
                                totalBlanksNeeded -= plate.Quantity;
                            }
                        }
                    }
                    else //just add 4 jobs to plate
                    {
                        plate.Quantity = sortedOrders[0].Qty;
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
                            sortedOrders.Remove(order);
                        }
                    }
                }

                if (totalBlanksNeeded > 200)
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {totalBlanksNeeded}");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {totalBlanksNeeded}, Press sheets used: {plateAssignments.Select(pa => pa.Plate).Distinct().Sum(p => p.Quantity)}");
                    return plateAssignments;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
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