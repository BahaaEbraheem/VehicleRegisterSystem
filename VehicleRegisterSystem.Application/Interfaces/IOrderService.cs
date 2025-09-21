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
        Task<ServiceResult<bool>> RegisterBoardAsync(Guid id, string boardNumber, string registrarId, string registrarName);
        Task<OrderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetForUserAsync(string userId);
        Task<IEnumerable<OrderDto>> GetByStatusesAsync(params OrderStatus[] statuses);
        Task<IEnumerable<OrderDto>> GetNewAndReturnedAndModifiedOrdersAsync();
        Task<ServiceResult<bool>> SubmitOrderAsync(Guid orderId, string userId, string userName); // ✅ جديد


    }
}
