using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ReachingOutDB.Data
{
    public class OrderAuditLogServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private AuthenticationStateProvider authenticationStateProvider;
        private UserServices userService;
        #endregion

        #region Constructor
        public OrderAuditLogServices(IDbContextFactory<AppDbContext> contextFactory, AuthenticationStateProvider authenticationStateProvider, UserServices userService  )
        {
            this.contextFactory = contextFactory;
            this.authenticationStateProvider = authenticationStateProvider;
            this.userService = userService;
        }
        #endregion

        public async Task LogOrderChangesAsync(Order originalOrder, Order updatedOrder)
        {
            var dbContext = contextFactory.CreateDbContext();
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
            dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderAuditLog>> GetLogsOfOrder(Order order)
        {
            var dbContext = contextFactory.CreateDbContext();
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
