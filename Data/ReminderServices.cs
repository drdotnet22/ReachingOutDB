using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    // CRUD for SmtpSetting and ReminderRule (+ its Conditions), used by the admin Razor pages.
    // Follows the same IDbContextFactory pattern as the other *Services classes in this project.
    public class ReminderServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        #endregion

        #region Constructor
        public ReminderServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        #region Smtp settings
        public async Task<SmtpSetting?> GetSmtpSettingAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.SmtpSettings.FirstOrDefaultAsync();
        }

        public async Task SaveSmtpSettingAsync(SmtpSetting setting)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            setting.UpdatedAt = DateTime.UtcNow;

            if (setting.Id == 0)
            {
                dbContext.SmtpSettings.Add(setting);
            }
            else
            {
                dbContext.Update(setting);
            }

            await dbContext.SaveChangesAsync();
        }
        #endregion

        #region Reminder rules
        public async Task<List<ReminderRule>> GetReminderRulesAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.ReminderRules
                .Include(r => r.Conditions)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task AddReminderRuleAsync(ReminderRule rule)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            ApplyScheduleDefaults(rule);
            dbContext.ReminderRules.Add(rule);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateReminderRuleAsync(ReminderRule rule)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            ApplyScheduleDefaults(rule);

            // Conditions are edited as a whole in-memory list on the client, so the simplest
            // reliable way to persist them is to replace the existing rows rather than trying
            // to diff old vs. new conditions.
            var existingConditions = await dbContext.ReminderConditions
                .Where(c => c.ReminderRuleId == rule.ReminderRuleId)
                .ToListAsync();
            dbContext.ReminderConditions.RemoveRange(existingConditions);

            dbContext.Entry(rule).State = EntityState.Modified;

            if (rule.Kind == ReminderKind.OrderCondition)
            {
                foreach (var condition in rule.Conditions)
                {
                    condition.ReminderRuleId = rule.ReminderRuleId;
                    dbContext.ReminderConditions.Add(condition);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteReminderRuleAsync(ReminderRule rule)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            dbContext.ReminderRules.Remove(new ReminderRule { ReminderRuleId = rule.ReminderRuleId });
            await dbContext.SaveChangesAsync();
        }

        // Scheduled rules need a NextRunUtc to fire from; if one hasn't been set explicitly,
        // start the clock from now using the rule's own interval.
        private static void ApplyScheduleDefaults(ReminderRule rule)
        {
            if (rule.Kind == ReminderKind.Scheduled && rule.NextRunUtc == null)
            {
                var value = rule.IntervalValue ?? 1;
                var unit = rule.IntervalUnit ?? ReminderIntervalUnit.Months;
                rule.NextRunUtc = unit == ReminderIntervalUnit.Months
                    ? DateTime.UtcNow.AddMonths(value)
                    : DateTime.UtcNow.AddDays(value);
            }
        }
        #endregion
    }
}
