using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    public class MiscSettingsServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public MiscSettingsServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<MiscSetting> GetMiscSettingsAsync()
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            var settingsList = await dbContext.MiscSettings.ToListAsync();
            return settingsList.FirstOrDefault();
        }


        public async Task UpdateMiscSettingsAsync(MiscSetting miscSetting)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Update(miscSetting);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
