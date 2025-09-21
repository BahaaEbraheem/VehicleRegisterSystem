using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs
{
    public class EditUserDto : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;  // not nullable
        public string Password { get; set; } = string.Empty;     // not nullable
        public string ConfirmPassword { get; set; } = string.Empty; // not nullable
        public UserRole Role { get; set; } = UserRole.User;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Only validate password if user typed something
            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult(
                        "تأكيد كلمة المرور مطلوب - Password confirmation is required",
                        new[] { nameof(ConfirmPassword) });
                }
                else if (Password != ConfirmPassword)
                {
                    yield return new ValidationResult(
                        "كلمة المرور وتأكيدها غير متطابقتين - Password and confirmation do not match",
                        new[] { nameof(ConfirmPassword) });
                }
            }
        }
    }

}
