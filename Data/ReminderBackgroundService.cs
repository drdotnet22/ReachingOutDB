using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    // Runs for the lifetime of the app. On a fixed poll interval it:
    //   1. Checks every enabled OrderCondition rule against current Orders, sending (and logging)
    //      one email per Order the first time it matches that rule.
    //   2. Checks every enabled Scheduled rule's NextRunUtc, sending a fixed email and advancing
    //      NextRunUtc when it's due, regardless of any Order data.
    public class ReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

        private readonly IDbContextFactory<AppDbContext> contextFactory;
        private readonly ReminderMailSender mailSender;
        private readonly ILogger<ReminderBackgroundService> logger;

        public ReminderBackgroundService(
            IDbContextFactory<AppDbContext> contextFactory,
            ReminderMailSender mailSender,
            ILogger<ReminderBackgroundService> logger)
        {
            this.contextFactory = contextFactory;
            this.mailSender = mailSender;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollInterval);
            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Reminder check failed.");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            await using var db = await contextFactory.CreateDbContextAsync(ct);

            var smtpSetting = await db.SmtpSettings.FirstOrDefaultAsync(ct);
            if (smtpSetting == null || string.IsNullOrWhiteSpace(smtpSetting.Host))
            {
                // SMTP hasn't been configured yet - nothing to do.
                return;
            }

            var rules = await db.ReminderRules
                .Include(r => r.Conditions)
                .Where(r => r.IsEnabled)
                .ToListAsync(ct);

            foreach (var rule in rules.Where(r => r.Kind == ReminderKind.OrderCondition))
            {
                await ProcessOrderConditionRuleAsync(db, rule, smtpSetting, ct);
            }

            foreach (var rule in rules.Where(r => r.Kind == ReminderKind.Scheduled))
            {
                await ProcessScheduledRuleAsync(db, rule, smtpSetting, ct);
            }
        }

        private async Task ProcessOrderConditionRuleAsync(AppDbContext db, ReminderRule rule, SmtpSetting smtp, CancellationToken ct)
        {
            if (rule.Conditions.Count == 0)
            {
                return;
            }

            var alreadySent = await db.ReminderLogs
                .Where(l => l.ReminderRuleId == rule.ReminderRuleId)
                .Select(l => l.OrderId)
                .ToListAsync(ct);
            var alreadySentSet = alreadySent.ToHashSet();

            var orders = await db.Orders.AsNoTracking().ToListAsync(ct);

            foreach (var order in orders)
            {
                if (alreadySentSet.Contains(order.OrderId))
                {
                    continue;
                }

                if (!ReminderConditionEvaluator.Matches(rule, order))
                {
                    continue;
                }

                try
                {
                    await mailSender.SendAsync(smtp, rule.RecipientEmails, rule.Subject, rule.Body, ct);
                    db.ReminderLogs.Add(new ReminderLog { ReminderRuleId = rule.ReminderRuleId, OrderId = order.OrderId });
                    await db.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send reminder '{RuleName}' for order {OrderId}.", rule.Name, order.OrderId);
                }
            }
        }

        private async Task ProcessScheduledRuleAsync(AppDbContext db, ReminderRule rule, SmtpSetting smtp, CancellationToken ct)
        {
            if (rule.NextRunUtc == null || rule.NextRunUtc > DateTime.UtcNow)
            {
                return;
            }

            try
            {
                await mailSender.SendAsync(smtp, rule.RecipientEmails, rule.Subject, rule.Body, ct);

                var now = DateTime.UtcNow;
                rule.LastRunUtc = now;
                rule.NextRunUtc = AddInterval(now, rule.IntervalValue ?? 1, rule.IntervalUnit ?? ReminderIntervalUnit.Months);
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send scheduled reminder '{RuleName}'.", rule.Name);
            }
        }

        private static DateTime AddInterval(DateTime start, int value, ReminderIntervalUnit unit) =>
            unit == ReminderIntervalUnit.Months ? start.AddMonths(value) : start.AddDays(value);
    }
}
