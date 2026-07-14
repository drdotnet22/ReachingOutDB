using Microsoft.EntityFrameworkCore;

namespace ReachingOutDB.Data
{
    // Runs for the lifetime of the app. On a fixed 5-minute poll interval it:
    //   1. For each enabled OrderCondition rule that is due (per its own CheckIntervalMinutes),
    //      checks current Orders and sends (and logs) one email per Order the first time it
    //      matches that rule.
    //   2. Checks every enabled Scheduled rule's NextRunUtc, sending a fixed email and advancing
    //      NextRunUtc when it's due, regardless of any Order data.
    //
    // Both paths "claim" their send in the database (via a unique index or a concurrency token)
    // BEFORE actually sending the email, and only send if the claim succeeds. This matters if
    // this service is ever running more than once at the same time against the same database -
    // e.g. two app instances, or a restart landing mid-poll - since without it, two overlapping
    // runs could both decide "this hasn't been sent yet" and both send.
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

            var checkIntervalMinutes = Math.Max(1, rule.CheckIntervalMinutes ?? 15);
            if (rule.LastCheckedUtc != null && rule.LastCheckedUtc.Value.AddMinutes(checkIntervalMinutes) > DateTime.UtcNow)
            {
                // Not due yet - this rule asked to be checked less often than the service's own poll tick.
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

                // Claim this (rule, order) pair FIRST by inserting the log row. The unique index
                // on (ReminderRuleId, OrderId) means if another run already claimed it a moment
                // ago, this insert fails and we skip sending - only the run that wins gets to send.
                var logEntry = new ReminderLog { ReminderRuleId = rule.ReminderRuleId, OrderId = order.OrderId };
                db.ReminderLogs.Add(logEntry);
                try
                {
                    await db.SaveChangesAsync(ct);
                }
                catch (DbUpdateException)
                {
                    // Someone else already claimed this order for this rule - not an error, just skip.
                    db.Entry(logEntry).State = EntityState.Detached;
                    continue;
                }

                try
                {
                    await mailSender.SendAsync(smtp, rule.RecipientEmails, rule.Subject, rule.Body, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send reminder '{RuleName}' for order {OrderId}.", rule.Name, order.OrderId);
                    // Sending failed after we claimed it - undo the claim so it's retried next poll.
                    db.ReminderLogs.Remove(logEntry);
                    await db.SaveChangesAsync(ct);
                }
            }

            // Record that this rule was checked, so the CheckIntervalMinutes gate above works.
            // ReminderRule.Version is a concurrency token, so if another run already recorded a
            // check for this same rule a moment ago, this just throws and we ignore it - harmless,
            // it only means we skip re-recording a check time someone else already set.
            rule.LastCheckedUtc = DateTime.UtcNow;
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Fine - another run already recorded this rule's check time.
            }
        }

        private async Task ProcessScheduledRuleAsync(AppDbContext db, ReminderRule rule, SmtpSetting smtp, CancellationToken ct)
        {
            if (rule.NextRunUtc == null || rule.NextRunUtc > DateTime.UtcNow)
            {
                return;
            }

            // Claim this run FIRST by advancing NextRunUtc. ReminderRule.Version is a concurrency
            // token (Postgres xmin), so if another run already updated this same row a moment ago,
            // this SaveChangesAsync throws DbUpdateConcurrencyException instead of overwriting it -
            // that's our signal that we lost the race, so we skip sending.
            var now = DateTime.UtcNow;
            rule.LastRunUtc = now;
            rule.NextRunUtc = AddInterval(now, rule.IntervalValue ?? 1, rule.IntervalUnit ?? ReminderIntervalUnit.Months);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else already claimed and sent this scheduled reminder - not an error, just skip.
                return;
            }

            try
            {
                await mailSender.SendAsync(smtp, rule.RecipientEmails, rule.Subject, rule.Body, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send scheduled reminder '{RuleName}'.", rule.Name);
                // We don't roll NextRunUtc back here - if sending is failing (e.g. SMTP is down),
                // retrying every 5 minutes until the next interval would just spam the log the
                // same way. It will pick back up on its next scheduled interval.
            }
        }

        private static DateTime AddInterval(DateTime start, int value, ReminderIntervalUnit unit) =>
            unit == ReminderIntervalUnit.Months ? start.AddMonths(value) : start.AddDays(value);
    }
}
