using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public class CustomerChangesServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public CustomerChangesServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<IEnumerable<CustomerChangesLog>> GetCustomerChangesLogsAsync(int customerId)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                return await dbContext.CustomerChangesLogs
                    .Where(c => c.CustomerId == customerId)
                    .OrderByDescending(c => c.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddCustomerChangesLogAsync(CustomerChangesLog log)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.CustomerChangesLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
