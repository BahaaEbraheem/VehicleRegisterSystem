using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.New;
        public DateTime? StatusChangedAt { get; set; }
        public string StatusChangedById { get; set; }
        public string StatusChangedByName { get; set; }

        public string BoardNumber { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string DeletedById { get; set; }
        public string DeletedByName { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string ModifiedById { get; set; }
        public string ModifiedByName { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
