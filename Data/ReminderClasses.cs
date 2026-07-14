namespace ReachingOutDB.Data
{
    // Single-row table holding the outgoing SMTP credentials used to send reminder emails.
    public class SmtpSetting
    {
        public int Id { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string FromAddress { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // A reminder either fires per-Order when it matches a set of conditions (OrderCondition),
    // or fires on a fixed calendar interval regardless of Order data (Scheduled).
    public class ReminderRule
    {
        public Guid ReminderRuleId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ReminderKind Kind { get; set; }
        public bool IsEnabled { get; set; } = true;

        // Comma-separated list of recipient email addresses.
        public string RecipientEmails { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // --- OrderCondition-only fields ---
        // How often (in minutes) the background service checks Orders against this rule's conditions.
        public int? CheckIntervalMinutes { get; set; } = 15;
        public ICollection<ReminderCondition> Conditions { get; set; } = new List<ReminderCondition>();

        // --- Scheduled-only fields ---
        public int? IntervalValue { get; set; }
        public ReminderIntervalUnit? IntervalUnit { get; set; }
        public DateTime? NextRunUtc { get; set; }
        public DateTime? LastRunUtc { get; set; }
    }

    // One condition belonging to an OrderCondition ReminderRule. All conditions on a rule
    // are combined with AND (an Order must match every condition to trigger the reminder).
    public class ReminderCondition
    {
        public Guid ReminderConditionId { get; set; } = Guid.NewGuid();
        public Guid ReminderRuleId { get; set; }
        public ReminderRule? ReminderRule { get; set; }

        // Must match a Name in ReminderOrderFields.All.
        public string FieldName { get; set; } = string.Empty;
        public ReminderOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    // Records that a given rule already sent a reminder for a given Order, so the same
    // Order never triggers the same OrderCondition rule twice.
    public class ReminderLog
    {
        public Guid ReminderLogId { get; set; } = Guid.NewGuid();
        public Guid ReminderRuleId { get; set; }
        public ReminderRule? ReminderRule { get; set; }
        public Guid OrderId { get; set; }
        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    }

    public enum ReminderKind
    {
        OrderCondition = 1,
        Scheduled = 2
    }

    public enum ReminderOperator
    {
        Equals = 1,
        NotEquals = 2,
        GreaterThan = 3,
        LessThan = 4
    }

    public enum ReminderIntervalUnit
    {
        Days = 1,
        Months = 2
    }
}
