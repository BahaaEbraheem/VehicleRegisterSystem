using VehicleRegisterSystem.Domain;

namespace VehicleRegisterSystem.Infrastructure.Repositories
{
    /// <summary>
    /// واجهة مستودع المستخدمين
    /// User repository interface
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        Task<IEnumerable<ApplicationUser>> GetAllAsync();

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        Task<ApplicationUser?> GetByIdAsync(int id);

        /// <summary>
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        Task<ApplicationUser?> GetByEmailAsync(string email, int? excludeUserId = null);

        /// <summary>
        /// الحصول على المستخدمين النشطين
        /// Get active users
        /// </summary>
        Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync();

        /// <summary>
        /// البحث عن المستخدمين
        /// Search users
        /// </summary>
        Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm);

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        Task<int> AddAsync(ApplicationUser user);

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        Task<bool> UpdateAsync(ApplicationUser user);

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        Task<bool> SetActiveStatusAsync(int id, bool isActive);

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);
    }


}
