using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Validation;

namespace VehicleRegisterSystem.Application.Interfaces
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// تسجيل الدخول
        /// Login
        /// </summary>
        Task<ServiceResult<LoggedInUserDto>> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// تسجيل مستخدم جديد
        /// Register new user
        /// </summary>
        Task<ServiceResult<string>> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// تغيير كلمة المرور
        /// Change password
        /// </summary>
        Task<ServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// التحقق من وجود البريد الإلكتروني
        /// Check if email exists
        /// </summary>
        Task<ServiceResult<bool>> EmailExistsAsync(string email);

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// التحقق من كلمة المرور
        /// Verify password hash
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }
}
