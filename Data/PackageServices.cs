using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public enum UspsPackageIssueType
    {
        // Customer has a USPS order this period but no shipping package on file.
        MissingPackage,
        // Sum of the customer's package quantities doesn't match the order's PostalQty.
        QuantityMismatch
    }

    // A validation finding tying a USPS order to a problem with its customer's packages.
    public class UspsPackageIssue
    {
        public Order Order { get; set; }
        public UspsPackageIssueType IssueType { get; set; }
        public int PostalQty { get; set; }
        public int PackagedQty { get; set; }
    }

    public class PackageServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public PackageServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<IEnumerable<PackageOption>> GetPackageOptionsAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.PackageOptions.OrderBy(p => p.PackageDescription).ToListAsync();
        }

        public async Task AddPackageOptionAsync(PackageOption packageOption)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.PackageOptions.Add(packageOption);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdatePackageOptionAsync(PackageOption packageOption)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Update(packageOption);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeletePackageOptionAsync(PackageOption packageOption)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.PackageOptions.Remove(packageOption);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Package>> GetPackagesAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Packages
                .Include(p => p.Customer)
                .Include(p => p.PackageOption)
                .OrderBy(p => p.ContactName)
                .ToListAsync();
        }

        // Packages for customers with a ready-to-ship USPS order in the given period. Because
        // packages are a per-customer profile, this returns every package for those customers
        // (covering the split-across-multiple-locations case).
        public async Task<IEnumerable<Package>> GetReadyToShipPackagesAsync(int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            var customerIds = dbContext.Orders
                .Where(o => o.Year == year && o.Quarter == quarter
                         && o.PostalQty > 0 && o.JobStatus == JobStatus.ReadyToShip)
                .Select(o => o.CustomerId);

            return await dbContext.Packages
                .Include(p => p.Customer)
                .Include(p => p.PackageOption)
                .Where(p => customerIds.Contains(p.CustomerId))
                .OrderBy(p => p.ContactName)
                .ToListAsync();
        }

        // Validates that every USPS order in the given period has shipping packages that reconcile
        // with its PostalQty. Missing packages are hard problems; quantity mismatches are softer
        // (a customer's order can legitimately split across multiple location packages).
        public async Task<List<UspsPackageIssue>> GetUspsPackageIssuesAsync(int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var uspsOrders = await dbContext.Orders
                .Include(o => o.Customer)
                .Where(o => o.Year == year && o.Quarter == quarter && o.PostalQty > 0)
                .ToListAsync();

            if (uspsOrders.Count == 0)
            {
                return new List<UspsPackageIssue>();
            }

            var customerIds = uspsOrders.Select(o => o.CustomerId).ToList();
            var packagedByCustomer = await dbContext.Packages
                .Where(p => customerIds.Contains(p.CustomerId))
                .GroupBy(p => p.CustomerId)
                .Select(g => new { CustomerId = g.Key, Qty = g.Sum(p => p.Qty) })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Qty);

            var issues = new List<UspsPackageIssue>();
            foreach (var order in uspsOrders)
            {
                var postalQty = order.PostalQty ?? 0;
                if (!packagedByCustomer.TryGetValue(order.CustomerId, out var packagedQty))
                {
                    issues.Add(new UspsPackageIssue
                    {
                        Order = order,
                        IssueType = UspsPackageIssueType.MissingPackage,
                        PostalQty = postalQty,
                        PackagedQty = 0
                    });
                }
                else if (packagedQty != postalQty)
                {
                    issues.Add(new UspsPackageIssue
                    {
                        Order = order,
                        IssueType = UspsPackageIssueType.QuantityMismatch,
                        PostalQty = postalQty,
                        PackagedQty = packagedQty
                    });
                }
            }
            return issues;
        }

        public async Task AddPackageAsync(Package package)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Packages.Add(package);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdatePackageAsync(Package package)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                var exists = await dbContext.Packages
                    .AsNoTracking()
                    .AnyAsync(p => p.PackageId == package.PackageId);
                if (!exists)
                {
                    dbContext.Packages.Add(package);
                }
                else
                {
                    dbContext.Update(package);
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeletePackageAsync(Package package)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Packages.Remove(package);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
