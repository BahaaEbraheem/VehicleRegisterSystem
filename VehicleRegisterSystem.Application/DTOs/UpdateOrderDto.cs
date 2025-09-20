using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleRegisterSystem.Application.DTOs
{
    public class UpdateOrderDto : CreateOrderDto
    {
        public Guid Id { get; set; }
    }
}
