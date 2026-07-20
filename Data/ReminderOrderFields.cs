namespace ReachingOutDB.Data
{
    // The small, curated list of Order fields that reminder rules are allowed to filter on.
    // Keeping this list explicit (rather than reflecting over every property on Order) means
    // the rule-builder UI only ever offers fields someone deliberately chose to expose here.
    // To make a new field available to reminder rules, add one entry below.
    public static class ReminderOrderFields
    {
        public class FieldDefinition
        {
            public string Name { get; init; } = string.Empty;
            public string DisplayName { get; init; } = string.Empty;
            // All values are compared as decimal so Equals/GreaterThan/LessThan work the same
            // way for every field, including enums (compared by their underlying number).
            public Func<Order, decimal> Selector { get; init; } = _ => 0;
        }

        public static readonly IReadOnlyList<FieldDefinition> All = new List<FieldDefinition>
        {
            new() { Name = "CustomerId", DisplayName = "Customer ID", Selector = o => o.CustomerId},
            new() { Name = "IntlQty", DisplayName = "Intl Qty", Selector = o => o.IntlQty ?? 0 },
            new() { Name = "Qty", DisplayName = "Qty", Selector = o => o.Qty },
            new() { Name = "DmQty", DisplayName = "DM Qty", Selector = o => o.DmQty ?? 0 },
            new() { Name = "UpsQty", DisplayName = "UPS Qty", Selector = o => o.UpsQty ?? 0 },
            new() { Name = "PostalQty", DisplayName = "USPS Qty", Selector = o => o.PostalQty ?? 0 },
            new() { Name = "LtlQty", DisplayName = "LTL Qty", Selector = o => o.LtlQty ?? 0 },
            new() { Name = "JobStatus", DisplayName = "Job Status", Selector = o => (int)o.JobStatus },
            new() { Name = "Archived", DisplayName = "Archived", Selector = o => o.Archived ? 1 : 0 },
            new() { Name = "DmCost", DisplayName = "DM Cost", Selector = o => o.DmCost ?? 0 },
            new() { Name = "UpsCost", DisplayName = "UPS Cost", Selector = o => o.UpsCost ?? 0 },
            new() { Name = "PostalCost", DisplayName = "USPS Cost", Selector = o => o.PostalCost ?? 0 },
            new() { Name = "IntlCost", DisplayName = "Intl Cost", Selector = o => o.IntlCost ?? 0 },
            new() { Name = "LTLCost", DisplayName = "LTL Cost", Selector = o => o.LTLCost ?? 0 },
        };

        public static FieldDefinition? Find(string name) => All.FirstOrDefault(f => f.Name == name);

        // Parses a condition's stored Value string into the same decimal scale its Selector
        // returns. JobStatus conditions may store either the enum name (e.g. "ReadyToShip")
        // or its numeric value; everything else is a plain number.
        public static decimal ParseValue(FieldDefinition field, string rawValue)
        {
            if (field.Name == "JobStatus" && Enum.TryParse<JobStatus>(rawValue, ignoreCase: true, out var status))
            {
                return (int)status;
            }

            return decimal.Parse(rawValue);
        }
    }

    public static class ReminderConditionEvaluator
    {
        public static bool Matches(ReminderRule rule, Order order)
        {
            if (rule.Conditions == null || rule.Conditions.Count == 0)
            {
                return false;
            }

            return rule.Conditions.All(condition => Evaluate(condition, order));
        }

        public static bool Evaluate(ReminderCondition condition, Order order)
        {
            var field = ReminderOrderFields.Find(condition.FieldName);
            if (field == null)
            {
                return false;
            }

            decimal actual = field.Selector(order);
            decimal expected;
            try
            {
                expected = ReminderOrderFields.ParseValue(field, condition.Value);
            }
            catch (FormatException)
            {
                return false;
            }

            return condition.Operator switch
            {
                ReminderOperator.Equals => actual == expected,
                ReminderOperator.NotEquals => actual != expected,
                ReminderOperator.GreaterThan => actual > expected,
                ReminderOperator.LessThan => actual < expected,
                _ => false
            };
        }
    }
}
