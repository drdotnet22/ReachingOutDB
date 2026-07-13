using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ReachingOutDB.Data
{
    public class AuditLogServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private UserServices userService;
        #endregion

        #region Constructor
        public AuditLogServices(IDbContextFactory<AppDbContext> contextFactory, UserServices userService  )
        {
            this.contextFactory = contextFactory;
            this.userService = userService;
        }
        #endregion

        // Diffs every public property on Order via reflection so new fields are
        // audited automatically without updating this method.
        public async Task LogOrderChangesAsync(Order originalOrder, Order updatedOrder)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            var properties = typeof(Order).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.Name == nameof(Order.OrderId)) continue;
                if (prop.Name == nameof(Order.Customer)) continue;

                var originalValue = prop.GetValue(originalOrder)?.ToString();
                var updatedValue = prop.GetValue(updatedOrder)?.ToString();

                if (originalValue != updatedValue)
                {
                    OrderAuditLog auditLog = new OrderAuditLog();
                    auditLog.OrderId = originalOrder.OrderId;
                    auditLog.OldValue = originalValue;
                    auditLog.NewValue = updatedValue;
                    auditLog.PropertyName = prop.Name;
                    auditLog.Action = "Updated";
                    auditLog.UserName = userService.CurrentUser.Name;
                    dbContext.OrderAuditLogs.Add(auditLog);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task<OrderAuditLog> LogOrderCreation(Order order)
        {
            return new OrderAuditLog()
            {
                OrderId = order.OrderId,
                Action = "Order created",
                UserName = userService.CurrentUser.Name
            };
        }

        public async Task<IEnumerable<OrderAuditLog>> GetLogsOfOrder(Order order)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                return await dbContext.OrderAuditLogs
                    .Where(l => l.Order == order)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
