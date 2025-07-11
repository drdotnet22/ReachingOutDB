using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
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
            var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.PackageOptions.OrderBy(p => p.PackageDescription).ToListAsync();
        }

        public async Task AddPackageOptionAsync(PackageOption packageOption)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
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
            var dbContext = await contextFactory.CreateDbContextAsync();
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
            var dbContext = await contextFactory.CreateDbContextAsync();
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
            var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Packages
                .Include(p => p.Customer)
                .Include(p => p.PackageOption)
                .OrderBy(p => p.ContactName)
                .ToListAsync();
        }

        public async Task AddPackageAsync(Package package)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
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
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Update(package);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeletePackageAsync(Package package)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
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
