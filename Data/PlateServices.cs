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
                var plates = new List<Plate>();
                var plateAssignments = new List<PlateAssignment>();
                int plateNumber = 0;
                int year = orders.FirstOrDefault().Year;
                Quarter quarter = orders.FirstOrDefault().Quarter;

                // Calculate number of blanks needed
                var lowQtyOrders = sortedOrders.Where(o => o.Qty <= 950).ToList();
                int totalBlanksNeeded = lowQtyOrders.Sum(o => o.Qty) + 1500;
                int blankCount = 0;

                int currentJobIndex = 0;

                while (sortedOrders[currentJobIndex + 1].Qty > 950)
                {
                    var plate = new Plate()
                    {
                        Year = year,
                        Quarter = quarter,
                        Number = plateNumber++
                    };

                    int platePosition = 0;

                    // Add blanks if needed
                    if (blankCount < totalBlanksNeeded)
                    {
                        blankCount += sortedOrders[currentJobIndex + 1].Qty;
                        plate.Quantity = sortedOrders[currentJobIndex + 1].Qty;

                        while (platePosition < 3)
                        {
                            var plateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = platePosition++,
                                OrderId = sortedOrders[currentJobIndex++].OrderId,
                                IsBlank = false
                            };
                            plateAssignments.Add(plateAssignment);
                            sortedOrders.Remove(sortedOrders[currentJobIndex]);
                        }
                        var blankPlateAssignment = new PlateAssignment()
                        {
                            Plate = plate,
                            Position = platePosition++,
                            IsBlank = true
                        };
                        plateAssignments.Add(blankPlateAssignment);
                    }
                    else //just add 4 jobs to plate
                    {
                        plate.Quantity = sortedOrders[currentJobIndex + 1].Qty;

                        while (platePosition < 4)
                        {
                            var plateAssignment = new PlateAssignment()
                            {
                                Plate = plate,
                                Position = platePosition++,
                                OrderId = sortedOrders[currentJobIndex++].OrderId,
                                IsBlank = false
                            };
                            plateAssignments.Add(plateAssignment);
                            sortedOrders.Remove(sortedOrders[currentJobIndex]);
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