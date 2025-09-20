using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs
{
    /// <summary>
    /// نموذج تسجيل الخروج
    /// Login model
    /// </summary>
    public class LogoutDto
    {
        public string CurrentUserName { get; set; } = string.Empty;
        public string CurrentUserEmail { get; set; } = string.Empty;
    }
}
