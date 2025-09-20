using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty; // IdentityUser uses string for PK
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? MembershipDate { get; set; } // يمكن أن يكون null في IdentityUser
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = "User"; // أو استخدم Enum
    }
}
