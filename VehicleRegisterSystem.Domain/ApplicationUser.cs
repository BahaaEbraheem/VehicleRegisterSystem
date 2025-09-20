using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public UserRole Role { get; set; } = UserRole.User; // default to User

    }
}
