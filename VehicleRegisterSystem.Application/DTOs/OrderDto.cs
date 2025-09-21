using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Application.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public string FullName { get; set; }
        public string NationalNumber { get; set; }
        public string MotherName { get; set; }
        public string CarName { get; set; }
        public string Model { get; set; }
        public int YearOfManufacture { get; set; }
        public string Color { get; set; }
        public string EngineNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? StatusChangedAt { get; set; }
        public string StatusChangedById { get; set; }
        public string StatusChangedByName { get; set; }
        public string BoardNumber { get; set; }
        // ✅ قائمة أسباب الإعادة السابقة
        // ✅ قائمة أسباب الإعادة السابقة
        public List<OrderReturnHistoryDto> ReturnHistory { get; set; }

        // ✅ سبب إعادة الطلب الحالي
        public string CurrentReturnComment { get; set; }

    }

    public class OrderReturnHistoryDto
    {
        public string Comment { get; set; }
        public DateTime ReturnedAt { get; set; }
        public string ReturnedByName { get; set; }
    }
}
