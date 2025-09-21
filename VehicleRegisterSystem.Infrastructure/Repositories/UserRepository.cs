using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Infrastructure.Repositories
{
    /// <summary>
    /// تنفيذ مستودع المستخدمين
    /// User repository implementation using ASP.NET Core Identity
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(ILogger<UserRepository> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        public async Task<string> AddAsync(ApplicationUser user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception("Failed to create user.");
            }

            return user.Id; // IdentityUser.Id هو string by default
        }

        /// <summary>
        /// حذف مستخدم
        /// Delete a user by ID
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if a user exists by email
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
            return user != null;
        }

        /// <summary>
        /// الحصول على جميع المستخدمين النشطين
        /// Get all active users (non-deleted)
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
        {
            return _userManager.Users.Where(u => u.LockoutEnabled == false).ToList();
        }

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return _userManager.Users.ToList();
        }

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get a user by email
        /// </summary>
        public async Task<ApplicationUser?> GetByEmailAsync(string email, int? excludeUserId = null)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
            if (user != null && excludeUserId.HasValue && user.Id == excludeUserId.ToString())
                return null;

            return user;
        }

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get a user by ID
        /// </summary>
        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        /// <summary>
        /// البحث عن المستخدمين
        /// Search users by email or name
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            searchTerm = searchTerm?.Trim().ToLower() ?? "";
            return _userManager.Users
                .Where(u => u.Email.ToLower().Contains(searchTerm) ||
                            u.FullName.ToLower().Contains(searchTerm))
                .ToList();
        }

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return false;

            user.LockoutEnabled = !isActive; // LockoutEnabled false = active
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update existing user
        /// </summary>
        public async Task<bool> UpdateAsync(ApplicationUser user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null) return false;

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        public async Task AssignRoleAsync(string userId, UserRole role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var roleName = role.ToString();
                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }
        }

        /// <summary>
        /// Get roles of a user
        /// </summary>
        public async Task<IEnumerable<UserRole>> GetRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Enumerable.Empty<UserRole>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Select(r => Enum.Parse<UserRole>(r));
        }
    }
}
