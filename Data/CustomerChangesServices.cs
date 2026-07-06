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
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                return await dbContext.CustomerChangesLogs
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.Customer)
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
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                // Ensure the timestamp is UTC before saving
                if (log.Timestamp.Kind != DateTimeKind.Utc)
                {
                    log.Timestamp = log.Timestamp.ToUniversalTime();
                }

                log.Customer = null; // Avoid EF trying to insert/update the Customer entity

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
