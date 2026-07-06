using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReachingOutDB.Components.Pages.Orders;
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

        public async Task<List<Plate>> GetFilteredPlatesAsync(int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
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
                return new List<Plate>();
            }
        }

        public async Task SavePlateAsync(Plate plate)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            // Detach orders to prevent duplicate key constraint errors
            foreach (var assignment in plate.PlateAssignments)
            {
                if (assignment.Order != null)
                {
                    assignment.OrderId = assignment.Order.OrderId;
                    assignment.Order = null;
                }
            }

            try
            {
                if (plate.PlateId == Guid.Empty)
                {
                    dbContext.Add(plate);
                }
                else
                {
                    dbContext.Update(plate);
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public async Task MarkPlatedAsync(Plate plate)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
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
        public async Task AutoAssignJobsToPlatesAsync(List<Order> unplatedOrders, int blanksUnplated, int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            List<List<PlateAssignment>> plateAssignmentLists = new();
            unplatedOrders = unplatedOrders
                .Where(o => o.Qty > 950)
                .OrderByDescending(o => o.Qty)
                .ToList();

            for (var i = 150; i < 1000; i+=3)
            {
                plateAssignmentLists.Add(await AssignJobsToPlatesHelperRandomAsync(unplatedOrders, blanksUnplated, year, quarter, i));
            }

            // Get all lists with the minimum number of distinct plates
            List<PlateAssignment> bestAssignment = plateAssignmentLists
                .Where(list => list != null)
                .OrderBy(list => list.Select(pa => pa.Plate).Distinct().Count())
                .ThenBy(list => list.Sum(pa => pa.Plate.Quantity))
                .First();

            dbContext.AddRange(bestAssignment.Select(pa => pa.Plate).Distinct());
            dbContext.AddRange(bestAssignment);
            await dbContext.SaveChangesAsync();
        }

        private async Task<List<PlateAssignment>> AssignJobsToPlatesHelperRandomAsync(List<Order> unplatedOrders, int blanksUnplated, int year, Quarter quarter, int threshold)
        {
            Random random = new();
            try
            {
                var plateAssignments = new List<PlateAssignment>();
                var unplatedOrdersCopy = new List<Order>(unplatedOrders);

                //int threshold = random.Next(4, 20) * 50;

                while (unplatedOrdersCopy.Count >= 4 && unplatedOrdersCopy[0].Qty > 980)
                {
                    var plate = new Plate()
                    {
                        Year = year,
                        Quarter = quarter
                    };

                    int platePosition = 0;
                    // If largest quantity is <threshold> x2 more than 2nd order: do first job 2 up
                    if (unplatedOrdersCopy[0].Qty > unplatedOrdersCopy[1].Qty + threshold * 2)
                    {
                        plate.Quantity = unplatedOrdersCopy[0].Qty / 2;
                        var plateAssignment1 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 1,
                            OrderId = unplatedOrdersCopy[0].OrderId,
                            IsBlank = false
                        };
                        plateAssignments.Add(plateAssignment1);

                        var plateAssignment2 = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = 2,
                            OrderId = unplatedOrdersCopy[0].OrderId,
                            IsBlank = false
                        };
                        unplatedOrdersCopy.Remove(unplatedOrdersCopy[0]);
                        plateAssignments.Add(plateAssignment2);

                        var candidates = unplatedOrdersCopy.Where(o => o.Qty <= plate.Quantity).Take(2).ToList();
                        //threshold = random.Next(4, 20) * 50;
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
                                unplatedOrdersCopy.Remove(order);
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
                                blanksUnplated -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed but not urgent
                    else if (unplatedOrdersCopy[0].Qty > 4000 && unplatedOrdersCopy[0].Qty < blanksUnplated + 2500)
                    {
                        plate.Quantity = unplatedOrdersCopy[0].Qty;
                        var candidates = unplatedOrdersCopy.Take(4).ToList();

                        //threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = unplatedOrdersCopy.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold ||
                                remaingOrdersQty > blanksUnplated + 1000)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    OrderId = order.OrderId,
                                    IsBlank = false
                                };
                                unplatedOrdersCopy.Remove(order);
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
                                blanksUnplated -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and more urgent
                    else if (unplatedOrdersCopy[0].Qty > 2500 && blanksUnplated > 0)
                    {
                        plate.Quantity = unplatedOrdersCopy[0].Qty;
                        var candidates = unplatedOrdersCopy.Take(4).ToList();

                        //threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            int remaingOrdersQty = unplatedOrdersCopy.Sum(o => o.Qty);
                            if (order.Qty > plate.Quantity - threshold ||
                                remaingOrdersQty > blanksUnplated + 1500)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    OrderId = order.OrderId,
                                    IsBlank = false
                                };
                                unplatedOrdersCopy.Remove(order);
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
                                blanksUnplated -= plate.Quantity;
                            }
                        }
                    }

                    // Blanks still needed and very urgent
                    else if (blanksUnplated > 0)
                    {
                        plate.Quantity = unplatedOrdersCopy[0].Qty;
                        var candidates = unplatedOrdersCopy.Take(4).ToList();

                        //threshold = random.Next(4, 20) * 50;
                        foreach (var order in candidates)
                        {
                            if (order.Qty > plate.Quantity - (threshold / 2) ||
                                blanksUnplated < 1)
                            {
                                var plateAssignment = new PlateAssignment()
                                {
                                    Plate = plate,
                                    Position = 1 + candidates.IndexOf(order),
                                    OrderId = order.OrderId,
                                    IsBlank = false
                                };
                                unplatedOrdersCopy.Remove(order);
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
                                blanksUnplated -= plate.Quantity;
                            }
                        }
                    }
                    else //just add 4 jobs to plate
                    {
                        plate.Quantity = unplatedOrdersCopy[0].Qty;
                        var candidates = unplatedOrdersCopy.Take(4).ToList();
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
                            unplatedOrdersCopy.Remove(order);
                        }
                    }
                }

                if (blanksUnplated > 200)
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {blanksUnplated}");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Threshold: {threshold}, Plates Used: {plateAssignments.Select(pa => pa.Plate).Distinct().Count()}, Blanks Still Needed: {blanksUnplated}, Press sheets used: {plateAssignments.Select(pa => pa.Plate).Distinct().Sum(p => p.Quantity)}");
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
            await using var dbContext = await contextFactory.CreateDbContextAsync();
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