using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Entities;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetByUserAsync(string userId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
        Task<bool> EngineNumberExistsAsync(string engineNumber, Guid? excludeOrderId = null);
        Task<bool> BoardNumberExistsAsync(string boardNumber, Guid? excludeOrderId = null);
    }
}
