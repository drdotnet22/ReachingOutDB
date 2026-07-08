using System.Collections.Concurrent;

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

        private readonly ConcurrentDictionary<int, string> _mailingNotesLocks = new();

        public bool TryAquireMailingNotesLock(int customerId, string userName)
            => _mailingNotesLocks.TryAdd(customerId, userName);

        public string? GetMailingNotesLockHolder(int customerId)
            => _mailingNotesLocks.TryGetValue(customerId, out var holder) ? holder : null;

        public void ReleaseMailingNotesLock(int customerId, string userName)
            => _mailingNotesLocks.TryRemove(new KeyValuePair<int, string>(customerId, userName));
    }
}
