using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace ReachingOutDB.Data
{
    public class UserServices
    {
        private readonly IJSRuntime _jsRuntime;
        private IDbContextFactory<AppDbContext> contextFactory;
        private UserProfile _currentUser = new() { UserProfileId = 0, Name = "Select User", Role = UserRole.UnAssigned, Active = false};
        public event Action? OnUserChanged;

        public UserServices(IJSRuntime jsRuntime, IDbContextFactory<AppDbContext> contextFactory)
        {
            _jsRuntime = jsRuntime;
            this.contextFactory = contextFactory;
        }

        public UserProfile CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                if (_currentUser != null)
                {
                    SaveUserToBrowserAsync();
                }
            }
        }

        public bool AdminMode { get; set; } = false;

        public async Task<List<UserProfile>> GetUserListAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.UserProfiles.ToListAsync();
        }

        public async Task InitializeAsync(string? defaultRole = null)
        {
            var userIdStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "selectedUserId");
            int userId = 1;
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var parsedId))
            {
                userId = parsedId;
            }
            await SelectUserAsync(userId);
        }
        public async Task SelectUserAsync(int userId)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            var user = await dbContext.UserProfiles.FirstOrDefaultAsync(u => u.UserProfileId == userId);
            CurrentUser = user;
            OnUserChanged?.Invoke();
        }

        private async Task SaveUserToBrowserAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "selectedUserId", CurrentUser.UserProfileId.ToString());
        }

        public async Task AddUserAsync(UserProfile user)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            dbContext.UserProfiles.Add(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserProfile user)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task DeleteUserAsync(UserProfile user)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Remove(user);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
