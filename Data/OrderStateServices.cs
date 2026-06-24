using System.Collections.Concurrent;

namespace ReachingOutDB.Data
{
    public class OrderStateServices
    {
        // Thread-safe dictionary to hold progress per quarter (e.g., "2026Q3")
        private readonly ConcurrentDictionary<string, QuarterlyProgress> _quarterlyStats = new();

        // Event now passes the specific quarter key that was updated
        public event Func<string, Task>? OnDuploProgressUpdated;

        public QuarterlyProgress GetProgressForQuarter(string quarterKey)
        {
            // Returns existing data, or initializes a blank one if it doesn't exist yet
            return _quarterlyStats.GetOrAdd(quarterKey, _ => new QuarterlyProgress());
        }

        public async Task UpdateDuploProgress(string quarterKey, IEnumerable<Order> orders)
        {
            if (orders == null || !orders.Any()) return;

            // Get or create the specific quarter container
            var progress = GetProgressForQuarter(quarterKey);

            // Update the stats for this quarter safely
            lock (progress)
            {
                progress.TotalOrders = orders.Sum(o => o.Qty);
                progress.AssembledOrders = orders.Where(o => o.JobStatus >= JobStatus.ReadyToShip).Sum(o => o.Qty);
                progress.DuploProgressPercentage = progress.TotalOrders > 0
                    ? (progress.AssembledOrders * 100) / progress.TotalOrders
                    : 0;
            }

            // Notify listeners, passing along which quarter changed
            if (OnDuploProgressUpdated != null)
            {
                var tasks = OnDuploProgressUpdated.GetInvocationList()
                    .Cast<Func<string, Task>>()
                    .Select(del => del(quarterKey));
                await Task.WhenAll(tasks);
            }
        }

        // Checks if the given quarter exists in the dictionary.
        public bool EnsureQuarterProgressAsync(string quarterKey)
        {
            if (_quarterlyStats.TryGetValue(quarterKey, out var progress))
            {
                if (progress.TotalOrders > 0)
                {
                    // If there are orders, we can consider it as having progress.
                    return true;
                }
                else
                {
                    // If there are no orders, we can consider it as not having progress.
                    return false;
                }
            }
            else
            {
                // If the quarter does not exist, we can consider it as not having progress.
                return false;
            }
        }
    }

    // Model class to hold data per quarter
    public class QuarterlyProgress
    {
        public int TotalOrders { get; internal set; } = 0;
        public int AssembledOrders { get; internal set; } = 0;
        public int DuploProgressPercentage { get; internal set; } = 0;
    }
}