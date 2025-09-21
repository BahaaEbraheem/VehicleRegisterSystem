using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Validation;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Enums;
using VehicleRegisterSystem.Infrastructure.Repositories;

namespace VehicleRegisterSystem.Application.Services
{
    /// <summary>
    /// خدمة المستخدمين
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;
        public UserService(IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,ILogger<UserService> logger)
        {
            _orderService=  orderService;
            _userManager = userManager;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// الحصول على جميع المستخدمين
        /// Get all users
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على جميع المستخدمين - Getting all users");
                var users = await _userRepository.GetAllAsync();
                _logger.LogDebug("تم الحصول على {Count} مستخدم - Retrieved users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين - Error getting users");
                throw;
            }
        }

        /// <summary>
        /// الحصول على مستخدم بالمعرف
        /// Get user by ID
        /// </summary>
        public async Task<ServiceResult<ApplicationUser>> GetUserByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", id);
                    return ServiceResult<ApplicationUser>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("الحصول على المستخدم {UserId} - Getting user by ID", id);
                var user = await _userRepository.GetByIdAsync(id);

                if (user==null)
                {
                    _logger.LogWarning("لم يتم العثور على المستخدم {UserId} - User not found", id);
                    return ServiceResult<ApplicationUser>.Failure("المستخدم غير موجود - User not found");
                }

                _logger.LogDebug("تم العثور على المستخدم {UserId} - User found", id);
                return ServiceResult<ApplicationUser>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم {UserId} - Error getting user", id);
                return ServiceResult<ApplicationUser>.Failure("خطأ في الحصول على المستخدم - Error getting user");
            }
        }

   

        /// <summary>
        /// إضافة مستخدم جديد
        /// Add a new user
        /// </summary>
        public async Task<ServiceResult<string>> AddUserAsync(ApplicationUser user, string password)
        {
            try
            {
                if (user == null)
                {
                    _logger.LogWarning("تم تمرير مستخدم فارغ - Null user provided");
                    return ServiceResult<string>.Failure("بيانات المستخدم مطلوبة - User data is required");
                }

                // تحقق من وجود بريد
                if (await _userRepository.ExistsByEmailAsync(user.Email))
                {
                    _logger.LogWarning("محاولة إضافة مستخدم بريد إلكتروني موجود: {Email}", user.Email);
                    return ServiceResult<string>.Failure("البريد الإلكتروني موجود بالفعل - Email already exists");
                }

                _logger.LogDebug("إضافة مستخدم جديد: {Email}", user.Email);

                // استخدم UserManager عشان ينشئ الهاش تلقائي
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("فشل في إنشاء المستخدم: {Errors}", errors);
                    return ServiceResult<string>.Failure("فشل في إضافة المستخدم - Failed to add user");
                }

                _logger.LogInformation("تم إضافة مستخدم جديد بالمعرف {UserId}", user.Id);
                return ServiceResult<string>.Success(user.Id); // Identity بيرجع string GUID
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة المستخدم");
                return ServiceResult<string>.Failure("خطأ في إضافة المستخدم - Error adding user");
            }
        }


        /// <summary>
        /// تحديث مستخدم موجود
        /// Update an existing user
        /// </summary>
        public async Task<ServiceResult<string>> UpdateUserAsync(string userId, RegisterDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || model == null)
                {
                    _logger.LogWarning("تم تمرير بيانات غير صحيحة للتحديث - Invalid input for update");
                    return ServiceResult<string>.Failure("بيانات المستخدم مطلوبة - User data is required");
                }

                // الحصول على المستخدم الحالي
                var existingUser = await _userManager.FindByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning("المستخدم غير موجود للتحديث: {UserId}", userId);
                    return ServiceResult<string>.Failure("المستخدم غير موجود - User not found");
                }

                // تحقق من عدم وجود مستخدم آخر بنفس البريد
                var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
                if (userWithSameEmail != null && userWithSameEmail.Id != userId)
                {
                    _logger.LogWarning("البريد الإلكتروني موجود بالفعل: {Email}", model.Email);
                    return ServiceResult<string>.Failure("البريد الإلكتروني موجود بالفعل - Email already exists");
                }

                // تحديث البيانات
                existingUser.FullName = $"{model.FirstName} {model.LastName}";
                existingUser.Email = model.Email;
                existingUser.UserName = model.Email;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.Role = model.Role;

                var updateResult = await _userManager.UpdateAsync(existingUser);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError("فشل في تحديث المستخدم: {Errors}", errors);
                    return ServiceResult<string>.Failure("فشل في تحديث المستخدم - Failed to update user");
                }

                // تحديث كلمة المرور إذا تم إدخال كلمة جديدة
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                    var passwordResult = await _userManager.ResetPasswordAsync(existingUser, token, model.Password);

                    if (!passwordResult.Succeeded)
                    {
                        var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                        _logger.LogError("فشل في تحديث كلمة المرور: {Errors}", errors);
                        return ServiceResult<string>.Failure("فشل في تحديث كلمة المرور - Failed to update password");
                    }
                }

                _logger.LogInformation("تم تحديث المستخدم بنجاح: {UserId}", userId);
                return ServiceResult<string>.Success(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المستخدم {UserId}", userId);
                return ServiceResult<string>.Failure("خطأ في تحديث المستخدم - Error updating user");
            }
        }


        /// <summary>
        /// حذف مستخدم
        /// Delete a user
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteUserAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح للحذف: {UserId} - Invalid user ID provided for deletion", id);
                    return ServiceResult<bool>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                // التحقق من وجود المستخدم
                var existingUser = await _userRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("محاولة حذف مستخدم غير موجود: {UserId} - Attempt to delete non-existing user", id);
                    return ServiceResult<bool>.Failure("المستخدم غير موجود - User not found");
                }

                // جلب كل الطلبات الخاصة بالمستخدم
                var userOrders = await _orderService.GetForUserAsync(id);

                // التحقق من أن كل الطلبات مسودة
                if (userOrders.Any(o => o.Status != OrderStatus.Draft))
                {
                    _logger.LogWarning("لا يمكن حذف المستخدم {UserId} لأن لديه طلبات ليست مسودة", id);
                    return ServiceResult<bool>.Failure("لا يمكن حذف المستخدم لأن لديه طلبات ليست مسودة - Cannot delete user with non-draft orders");
                }
                _logger.LogDebug("حذف المستخدم {UserId} - Deleting user", id);
                var success = await _userRepository.DeleteAsync(id);

                if (success)
                {
                    _logger.LogInformation("تم حذف المستخدم {UserId} بنجاح - User deleted successfully", id);
                }
                else
                {
                    _logger.LogWarning("فشل في حذف المستخدم {UserId} - Failed to delete user", id);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId} - Error deleting user", id);
                return ServiceResult<bool>.Failure("خطأ في حذف المستخدم - Error deleting user");
            }
        }



    
       
  

    
    }
}
