using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public class ShippingSettingsServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public ShippingSettingsServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<IEnumerable<ShippingSetting>> GetShippingSettingsAsync()
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.ShippingSettings.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task AddShippingSettingAsync(ShippingSetting shippingSetting)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                shippingSetting.UpdatedAt = DateTime.UtcNow;
                dbContext.ShippingSettings.Add(shippingSetting);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateShippingSettingAsync(ShippingSetting shippingSetting)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                shippingSetting.UpdatedAt = DateTime.UtcNow;
                dbContext.Update(shippingSetting);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteShippingSettingAsync(ShippingSetting shippingSetting)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.ShippingSettings.Remove(shippingSetting);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}