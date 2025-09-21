using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Validation;
using VehicleRegisterSystem.Domain;
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
        public UserService(UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,ILogger<UserService> logger)
        {
            _userManager= userManager;
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
        /// الحصول على مستخدم بالبريد الإلكتروني
        /// Get user by email
        /// </summary>
        public async Task<ServiceResult<ApplicationUser>> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("تم تمرير بريد إلكتروني فارغ - Empty email provided");
                    return ServiceResult<ApplicationUser>.Failure("البريد الإلكتروني مطلوب - Email is required");
                }

                _logger.LogDebug("الحصول على المستخدم بالبريد الإلكتروني {Email} - Getting user by email", email);
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("لم يتم العثور على المستخدم بالبريد الإلكتروني {Email} - User not found by email", email);
                    return ServiceResult<ApplicationUser>.Failure("المستخدم غير موجود - User not found");
                }

                _logger.LogDebug("تم العثور على المستخدم بالبريد الإلكتروني {Email} - User found by email", email);
                return ServiceResult<ApplicationUser>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدم بالبريد الإلكتروني {Email} - Error getting user by email", email);
                return ServiceResult<ApplicationUser>.Failure("خطأ في الحصول على المستخدم - Error getting user");
            }
        }

        /// <summary>
        /// الحصول على المستخدمين النشطين
        /// Get active users
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync()
        {
            try
            {
                _logger.LogDebug("الحصول على المستخدمين النشطين - Getting active users");
                var users = await _userRepository.GetActiveUsersAsync();
                _logger.LogDebug("تم الحصول على {Count} مستخدم نشط - Retrieved active users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحصول على المستخدمين النشطين - Error getting active users");
                throw;
            }
        }

        /// <summary>
        /// البحث عن المستخدمين
        /// Search users
        /// </summary>
        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogDebug("مصطلح البحث فارغ، إرجاع جميع المستخدمين - Empty search term, returning all users");
                    return await GetAllUsersAsync();
                }

                _logger.LogDebug("البحث عن المستخدمين بالمصطلح: {SearchTerm} - Searching users with term", searchTerm);
                var users = await _userRepository.SearchUsersAsync(searchTerm);
                _logger.LogDebug("تم العثور على {Count} مستخدم - Found users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في البحث عن المستخدمين - Error searching users");
                throw;
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

        /// <summary>
        /// تفعيل أو إلغاء تفعيل مستخدم
        /// Activate or deactivate a user
        /// </summary>
        public async Task<ServiceResult<bool>> SetActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("تم تمرير معرف مستخدم غير صحيح: {UserId} - Invalid user ID provided", id);
                    return ServiceResult<bool>.Failure("معرف المستخدم غير صحيح - Invalid user ID");
                }

                _logger.LogDebug("تغيير حالة المستخدم {UserId} إلى {IsActive} - Changing user status to", id, isActive);
                var success = await _userRepository.SetActiveStatusAsync(id, isActive);

                if (success)
                {
                    _logger.LogInformation("تم تغيير حالة المستخدم {UserId} إلى {IsActive} - User status changed to", id, isActive);
                }
                else
                {
                    _logger.LogWarning("فشل في تغيير حالة المستخدم {UserId} - Failed to change user status", id);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تغيير حالة المستخدم {UserId} - Error changing user status", id);
                return ServiceResult<bool>.Failure("خطأ في تغيير حالة المستخدم - Error changing user status");
            }
        }

        /// <summary>
        /// التحقق من وجود مستخدم بالبريد الإلكتروني
        /// Check if user exists by email
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return false;
                }

                return await _userRepository.ExistsByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود المستخدم بالبريد الإلكتروني {Email} - Error checking user existence by email", email);
                return false;
            }
        }

        /// <summary>
        /// التحقق من صحة بيانات المستخدم
        /// Validate user data
        /// </summary>
        private ServiceResult<bool> ValidateUser(ApplicationUser user)
        {
            //if (string.IsNullOrWhiteSpace(user.FirstName))
            //{
            //    return ServiceResult<bool>.Failure("الاسم الأول مطلوب - First name is required");
            //}

          

            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// التحقق من صحة تنسيق البريد الإلكتروني
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibraryManagementSalt"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// التحقق من كون كلمة المرور مشفرة بالفعل
        /// Check if password is already hashed
        /// </summary>
        private bool IsPasswordHashed(string password)
        {
            // كلمة المرور المشفرة تكون عادة أطول من 40 حرف وتحتوي على أحرف Base64
            // Hashed passwords are usually longer than 40 characters and contain Base64 characters
            if (string.IsNullOrEmpty(password) || password.Length < 40)
                return false;

            try
            {
                // محاولة فك تشفير Base64 للتحقق من صحة التنسيق
                // Try to decode Base64 to verify format
                Convert.FromBase64String(password);
                return true;
            }
            catch
            {
                return false;
            }
        }

    
    }
}
