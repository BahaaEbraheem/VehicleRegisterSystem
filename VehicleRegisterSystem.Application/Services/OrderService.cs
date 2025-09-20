using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Entities;
using VehicleRegisterSystem.Domain.Enums;
using VehicleRegisterSystem.Infrastructure.Repositories;

namespace VehicleRegisterSystem.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IMemoryCache _cache;
        public OrderService(IOrderRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto, string userId, string userName)
        {
            // duplication check
            if (await _repo.EngineNumberExistsAsync(dto.EngineNumber))
                throw new InvalidOperationException("Duplicate engine number");

            var order = new Order
            {
                CreatedById = userId,
                CreatedByName = userName,
                FullName = dto.FullName,
                NationalNumber = dto.NationalNumber,
                MotherName = dto.MotherName,
                CarName = dto.CarName,
                Model = dto.Model,
                YearOfManufacture = dto.YearOfManufacture,
                Color = dto.Color,
                EngineNumber = dto.EngineNumber,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.New
            };

            await _repo.AddAsync(order);
            _cache.Remove($"user_orders_{userId}");
            return Map(order);
        }

        public async Task DeleteAsync(Guid id, string userId, string userName)
        {
            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned)
                throw new InvalidOperationException("Cannot delete in current state");
            // soft delete
            order.DeletedAt = DateTime.UtcNow;
            order.DeletedById = userId;
            order.DeletedByName = userName;
            await _repo.UpdateAsync(order);
            _cache.Remove($"order_{id}");
            _cache.Remove($"user_orders_{order.CreatedById}");
        }

        public async Task<OrderDto> GetByIdAsync(Guid id)
        {
            var cacheKey = $"order_{id}";
            if (_cache.TryGetValue(cacheKey, out OrderDto cached)) return cached;

            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            var dto = Map(order);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(3));
            return dto;
        }

        public async Task<IEnumerable<OrderDto>> GetForUserAsync(string userId)
        {
            var cacheKey = $"user_orders_{userId}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderDto> cached)) return cached;

            var list = await _repo.GetByUserAsync(userId);
            var dtos = list.Select(Map).ToList();
            _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(2));
            return dtos;
        }

        public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status)
        {
            var orders = await _repo.GetByStatusAsync(status); // _repo يستقبل Enum
            return orders.Select(Map); // تحويل Entities إلى DTO
        }

        public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto dto, string userId, string userName)
        {
            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned)
                throw new InvalidOperationException("Cannot edit in current state");

            if (await _repo.EngineNumberExistsAsync(dto.EngineNumber, id))
                throw new InvalidOperationException("Duplicate engine number");

            order.FullName = dto.FullName;
            order.NationalNumber = dto.NationalNumber;
            order.MotherName = dto.MotherName;
            order.CarName = dto.CarName;
            order.Model = dto.Model;
            order.YearOfManufacture = dto.YearOfManufacture;
            order.Color = dto.Color;
            order.EngineNumber = dto.EngineNumber;
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedById = userId;
            order.ModifiedByName = userName;

            await _repo.UpdateAsync(order);
            _cache.Remove($"order_{id}");
            _cache.Remove($"user_orders_{order.CreatedById}");
            return Map(order);
        }

        public async Task ReturnToUserAsync(Guid id, string validatorId, string validatorName, string comment)
        {
            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            if (order.Status == OrderStatus.Approved)
                throw new InvalidOperationException("Already approved");

            order.Status = OrderStatus.Returned;
            order.StatusChangedAt = DateTime.UtcNow;
            order.StatusChangedById = validatorId;
            order.StatusChangedByName = validatorName;
            // comment can be stored in audit in real impl
            await _repo.UpdateAsync(order);
            _cache.Remove($"order_{id}");
            _cache.Remove($"user_orders_{order.CreatedById}");
        }

        public async Task SetInProgressAsync(Guid id, string validatorId, string validatorName)
        {
            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            if (order.Status == OrderStatus.Approved)
                throw new InvalidOperationException("Already approved");

            order.Status = OrderStatus.InProgress;
            order.StatusChangedAt = DateTime.UtcNow;
            order.StatusChangedById = validatorId;
            order.StatusChangedByName = validatorName;
            await _repo.UpdateAsync(order);
            _cache.Remove($"order_{id}");
            _cache.Remove($"user_orders_{order.CreatedById}");
        }

        public async Task<bool> RegisterBoardAsync(Guid id, string boardNumber, string registrarId, string registrarName)
        {
            var order = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Order not found");
            if (order.Status != OrderStatus.InProgress)
                return false;

            if (await _repo.BoardNumberExistsAsync(boardNumber, id))
                return false;

            order.BoardNumber = boardNumber;
            order.Status = OrderStatus.Approved;
            order.StatusChangedAt = DateTime.UtcNow;
            order.StatusChangedById = registrarId;
            order.StatusChangedByName = registrarName;

            await _repo.UpdateAsync(order);

            _cache.Remove($"order_{id}");
            _cache.Remove($"user_orders_{order.CreatedById}");

            return true;
        }

        private OrderDto Map(Order o) => new OrderDto
        {
            Id = o.Id,
            CreatedById = o.CreatedById,
            CreatedByName = o.CreatedByName,
            FullName = o.FullName,
            NationalNumber = o.NationalNumber,
            MotherName = o.MotherName,
            CarName = o.CarName,
            Model = o.Model,
            YearOfManufacture = o.YearOfManufacture,
            Color = o.Color,
            EngineNumber = o.EngineNumber,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            StatusChangedAt = o.StatusChangedAt,
            StatusChangedById = o.StatusChangedById,
            StatusChangedByName = o.StatusChangedByName,
            BoardNumber = o.BoardNumber
        };

     
    }
}
