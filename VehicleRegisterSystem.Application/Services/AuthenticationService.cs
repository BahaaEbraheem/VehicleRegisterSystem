﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Validation;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Infrastructure.Repositories;

namespace VehicleRegisterSystem.Application.Services
{
    /// <summary>
    /// خدمة المصادقة
    /// Authentication service
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IJwtService _jwtService;

        public AuthenticationService(IJwtService jwtService, IUserRepository userRepository, ILogger<AuthenticationService> logger)
        {
            _jwtService = jwtService;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تسجيل الدخول
        /// Login
        /// </summary>
        public async Task<ServiceResult<LoggedInUserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogDebug("محاولة تسجيل دخول للمستخدم: {Email} - Login attempt for user: {Email}",
                    loginDto.Email, loginDto.Email);

                // البحث عن المستخدم بالبريد الإلكتروني
                // Find user by email
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Email.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                {
                    _logger.LogWarning("فشل تسجيل الدخول: المستخدم غير موجود - Login failed: User not found. Email: {Email}",
                        loginDto.Email);
                    return ServiceResult<LoggedInUserDto>.Failure("البريد الإلكتروني أو كلمة المرور غير صحيحة - Invalid email or password");
                }

            

                // التحقق من كلمة المرور
                // Verify password
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {

                    return ServiceResult<LoggedInUserDto>.Failure("البريد الإلكتروني أو كلمة المرور غير صحيحة - Invalid email or password");
                }

                // إنشاء نموذج المستخدم المسجل دخوله
                // Create logged in user model
                var loggedInUser = new LoggedInUserDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                };

                // ✅ إنشاء رمز JWT وإضافته إلى النموذج
                loggedInUser.Token = _jwtService.GenerateToken(loggedInUser);


                _logger.LogInformation("تم تسجيل الدخول بنجاح للمستخدم: {Email} - Successful login for user: {Email}",
                    loginDto.Email, loginDto.Email);

                return ServiceResult<LoggedInUserDto>.Success(loggedInUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل الدخول - Error during login");
                return ServiceResult<LoggedInUserDto>.Failure("حدث خطأ أثناء تسجيل الدخول - An error occurred during login");
            }
        }

        /// <summary>
        /// تسجيل مستخدم جديد
        /// Register new user
        /// </summary>
        public async Task<ServiceResult<int>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogDebug("محاولة تسجيل مستخدم جديد: {Email} - Attempting to register new user: {Email}",
                    registerDto.Email, registerDto.Email);

                // التحقق من وجود البريد الإلكتروني
                // Check if email exists
                var emailExistsResult = await EmailExistsAsync(registerDto.Email);
                if (emailExistsResult.IsSuccess && emailExistsResult.Data)
                {
                    _logger.LogWarning("فشل التسجيل: البريد الإلكتروني موجود مسبقاً - Registration failed: Email already exists. Email: {Email}",
                        registerDto.Email);
                    return ServiceResult<int>.Failure("البريد الإلكتروني موجود مسبقاً - Email already exists");
                }

                // إنشاء مستخدم جديد
                // Create new user
                var user = new ApplicationUser
                {
                    Email = registerDto.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = registerDto.PhoneNumber?.Trim(),
                    PasswordHash = HashPassword(registerDto.Password),
                 
                };

                var userId = await _userRepository.AddAsync(user);

                _logger.LogInformation("تم تسجيل مستخدم جديد بنجاح: {Email}, UserId: {UserId} - Successfully registered new user: {Email}, UserId: {UserId}",
                    registerDto.Email, userId, registerDto.Email, userId);

                return ServiceResult<int>.Success(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل مستخدم جديد - Error during user registration");
                return ServiceResult<int>.Failure("حدث خطأ أثناء تسجيل المستخدم - An error occurred during user registration");
            }
        }

        /// <summary>
        /// تغيير كلمة المرور
        /// Change password
        /// </summary>
        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                _logger.LogDebug("محاولة تغيير كلمة المرور للمستخدم: {UserId} - Attempting to change password for user: {UserId}",
                    userId, userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Failure("المستخدم غير موجود - User not found");
                }

                // التحقق من كلمة المرور الحالية
                // Verify current password
                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("فشل تغيير كلمة المرور: كلمة المرور الحالية خاطئة - Password change failed: Current password is incorrect. UserId: {UserId}",
                        userId);
                    return ServiceResult<bool>.Failure("كلمة المرور الحالية غير صحيحة - Current password is incorrect");
                }

                // تحديث كلمة المرور
                // Update password
                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);

                var success = await _userRepository.UpdateAsync(user);

                if (success)
                {
                    _logger.LogInformation("تم تغيير كلمة المرور بنجاح للمستخدم: {UserId} - Successfully changed password for user: {UserId}",
                        userId, userId);
                }

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تغيير كلمة المرور - Error during password change");
                return ServiceResult<bool>.Failure("حدث خطأ أثناء تغيير كلمة المرور - An error occurred during password change");
            }
        }

        /// <summary>
        /// التحقق من وجود البريد الإلكتروني
        /// Check if email exists
        /// </summary>
        public async Task<ServiceResult<bool>> EmailExistsAsync(string email)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var exists = users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));


                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التحقق من وجود البريد الإلكتروني - Error checking email existence");
                return ServiceResult<bool>.Failure("حدث خطأ أثناء التحقق من البريد الإلكتروني - An error occurred while checking email");
            }
        }

        /// <summary>
        /// تشفير كلمة المرور
        /// Hash password
        /// </summary>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibraryManagementSalt"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// التحقق من كلمة المرور
        /// Verify password hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}
