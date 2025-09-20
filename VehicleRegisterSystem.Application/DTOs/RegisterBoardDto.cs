using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs
{
    public class RegisterBoardDto
    {
        public Guid OrderId { get; set; }
        public string CarName { get; set; }
        public string Model { get; set; }
        public string EngineNumber { get; set; }
        public string BoardNumber { get; set; } // سيتم إدخاله يدوياً
    }
}
