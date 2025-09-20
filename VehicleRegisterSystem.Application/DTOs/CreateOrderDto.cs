using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs
{
    public class CreateOrderDto
    {
        public string FullName { get; set; }
        public string NationalNumber { get; set; }
        public string MotherName { get; set; }

        public string CarName { get; set; }
        public string Model { get; set; }
        public int YearOfManufacture { get; set; }
        public string Color { get; set; }

        public string EngineNumber { get; set; }
    }
}
