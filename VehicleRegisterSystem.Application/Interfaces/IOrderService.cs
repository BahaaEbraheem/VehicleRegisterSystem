using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Validation;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, string userId, string userName);
        Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto dto, string userId, string userName);
        Task DeleteAsync(Guid id, string userId, string userName);
        Task<ServiceResult<bool>> ReturnToUserAsync(Guid id, string validatorId, string validatorName, string comment);
        Task<ServiceResult<bool>> SetInProgressAsync(Guid id, string validatorId, string validatorName);
        Task<bool> RegisterBoardAsync(Guid id, string boardNumber, string registrarId, string registrarName);
        Task<OrderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetForUserAsync(string userId);
        Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status); // <-- تعديل هنا

    }
}
