using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Application.Services
{
    /// <summary>
    /// خدمة JWT للمصادقة والتفويض
    /// JWT service for authentication and authorization
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// منشئ خدمة JWT
        /// JWT service constructor
        /// </summary>
        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenHandler = new JwtSecurityTokenHandler();

            // إعداد معاملات التحقق من الرمز
            // Setup token validation parameters
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// إنشاء رمز JWT للمستخدم المسجل دخوله
        /// Generate JWT token for logged in user
        /// </summary>
        public string GenerateToken(LoggedInUserDto loggedInUser)
        {
            try
            {
                _logger.LogDebug("إنشاء رمز JWT للمستخدم {UserId} - Generating JWT token for user", loggedInUser.UserId);

                var claims = new List<Claim>
                {
                   new(ClaimTypes.NameIdentifier, loggedInUser.UserId.ToString()),
    new(ClaimTypes.Email, loggedInUser.Email),
    new(ClaimTypes.Name, loggedInUser.FullName),
    new(ClaimTypes.Role, loggedInUser.Role.ToString()), // now properly set
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
 };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = credentials
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);

                _logger.LogDebug("تم إنشاء رمز JWT بنجاح للمستخدم {UserId} - JWT token generated successfully for user", loggedInUser.UserId);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء رمز JWT للمستخدم {UserId} - Error generating JWT token for user", loggedInUser.UserId);
                throw;
            }
        }

    }
}
