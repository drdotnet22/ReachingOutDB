using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public enum IntlPackageIssueType
    {
        // Customer has an international order this period but no international package on file.
        MissingPackage,
        // Sum of the customer's international package quantities doesn't match the order's IntlQty.
        QuantityMismatch,
        // Customer has a shipping package this quarter but no INTL order.
        ExtraPackage
    }

    // A validation finding tying an international order to a problem with its customer's packages.
    public class IntlPackageIssue
    {
        public Order Order { get; set; }
        public IntlPackageIssueType IssueType { get; set; }
        public int IntlQty { get; set; }
        public int PackagedQty { get; set; }
    }

    public class IntlPackageServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public IntlPackageServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<IEnumerable<IntlPackage>> GetIntlPackagesAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.IntlPackages
                .Include(p => p.Customer)
                .OrderBy(p => p.ContactName)
                .ToListAsync();
        }

        // Validates that every international order in the given period has international packages that
        // reconcile with its IntlQty. Missing packages are hard problems; quantity mismatches are softer
        // (a customer's order can legitimately split across multiple location packages).
        public async Task<List<IntlPackageIssue>> GetIntlPackageIssuesAsync(int year, Quarter quarter)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var intlOrders = await dbContext.Orders
                .Include(o => o.Customer)
                .Where(o => o.Year == year && o.Quarter == quarter && o.IntlQty > 0)
                .ToListAsync();

            if (intlOrders.Count == 0)
            {
                return new List<IntlPackageIssue>();
            }

            var customerIds = intlOrders.Select(o => o.CustomerId).ToList();
            var allPackages = await dbContext.IntlPackages.Include(p => p.Customer).ToListAsync();
            var packagedByCustomer = await dbContext.IntlPackages
                .Where(p => customerIds.Contains(p.CustomerId))
                .GroupBy(p => p.CustomerId)
                .Select(g => new { CustomerId = g.Key, Qty = g.Sum(p => p.Qty) })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Qty);

            var issues = new List<IntlPackageIssue>();
            foreach (var order in intlOrders)
            {
                var intlQty = order.IntlQty ?? 0;
                if (!packagedByCustomer.TryGetValue(order.CustomerId, out var packagedQty))
                {
                    issues.Add(new IntlPackageIssue
                    {
                        Order = order,
                        IssueType = IntlPackageIssueType.MissingPackage,
                        IntlQty = intlQty,
                        PackagedQty = 0
                    });
                }
                else if (packagedQty != intlQty)
                {
                    issues.Add(new IntlPackageIssue
                    {
                        Order = order,
                        IssueType = IntlPackageIssueType.QuantityMismatch,
                        IntlQty = intlQty,
                        PackagedQty = packagedQty
                    });
                }
            }

            foreach (var package in allPackages)
            {
                if (!intlOrders.Any(o => o.CustomerId == package.CustomerId))
                {
                    issues.Add(new IntlPackageIssue
                    {
                        Order = new Order() { Customer = package.Customer, CustomerId = package.CustomerId },
                        IssueType = IntlPackageIssueType.ExtraPackage,
                        IntlQty = 0,
                        PackagedQty = package.Qty
                    });
                }
            }
            return issues;
        }

        public async Task AddIntlPackageAsync(IntlPackage intlPackage)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.IntlPackages.Add(intlPackage);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateIntlPackageAsync(IntlPackage intlPackage)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                var exists = await dbContext.IntlPackages
                    .AsNoTracking()
                    .AnyAsync(p => p.IntlPackageId == intlPackage.IntlPackageId);
                if (!exists)
                {
                    dbContext.IntlPackages.Add(intlPackage);
                }
                else
                {
                    dbContext.Update(intlPackage);
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteIntlPackageAsync(IntlPackage intlPackage)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.IntlPackages.Remove(intlPackage);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
