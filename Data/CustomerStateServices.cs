namespace ReachingOutDB.Data
{
    public class CustomerStateServices
    {
        public event Func<int, Task>? OnCustomerUpdated;

        public async Task NotifyCustomerUpdated(int customerId)
        {
            if (OnCustomerUpdated != null)
                await Task.WhenAll(OnCustomerUpdated.GetInvocationList()
                    .Cast<Func<int, Task>>()
                    .Select(d => d(customerId)));
        }
    }
}
