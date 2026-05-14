namespace ReachingOutDB.Data
{
    public class OrderStateServices
    {
        public int TotalOrders { get; private set; } = 0;
        public int AssembledOrders { get; private set; } = 0; 
        public int DuploProgressPercentage { get; set; } = 0;

        public event Func<Task>? OnDuploProgressUpdated;

        public async Task UpdateDuploProgress(IEnumerable<Order> orders)
        {
            if (orders == null || !orders.Any()) return;

            TotalOrders = orders.Sum(o => o.Qty);
            AssembledOrders = orders.Where(o => o.JobStatus >= JobStatus.ReadyToShip).Sum(o => o.Qty);
            DuploProgressPercentage = TotalOrders > 0 ? (AssembledOrders * 100) / TotalOrders : 0;

            if (OnDuploProgressUpdated != null)
            {
                var tasks = OnDuploProgressUpdated.GetInvocationList()
                    .Cast<Func<Task>>()
                    .Select(del => del());
                await Task.WhenAll(tasks);
            }
        }
    }
}
