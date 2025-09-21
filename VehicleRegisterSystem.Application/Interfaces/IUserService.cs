using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Validation;
using VehicleRegisterSystem.Domain;

namespace VehicleRegisterSystem.Application.Interfaces
{
    /// <summary>
    /// واجهة خدمة المستخدمين
    /// User service interface
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id);

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        Task<ServiceResult<string>> AddUserAsync(ApplicationUser user, string password);

        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        Task<ServiceResult<string>> UpdateUserAsync(string userId, RegisterDto user);

        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        Task<ServiceResult<bool>> DeleteUserAsync(string id);
    }
}
