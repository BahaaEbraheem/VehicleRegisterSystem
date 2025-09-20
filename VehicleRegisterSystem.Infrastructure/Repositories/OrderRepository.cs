using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain.Entities;
using VehicleRegisterSystem.Domain.Enums;
using VehicleRegisterSystem.Infrastructure.Data;

namespace VehicleRegisterSystem.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        public OrderRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Order order)
        {
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order)
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            return await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _db.Orders.Where(o => o.Status == status).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(string userId)
        {
            return await _db.Orders.Where(o => o.CreatedById == userId).AsNoTracking().ToListAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> EngineNumberExistsAsync(string engineNumber, Guid? excludeOrderId = null)
        {
            if (string.IsNullOrWhiteSpace(engineNumber)) return false;
            var q = _db.Orders.AsQueryable().Where(o => o.EngineNumber == engineNumber);
            if (excludeOrderId.HasValue) q = q.Where(o => o.Id != excludeOrderId.Value);
            return await q.AnyAsync();
        }

        public async Task<bool> BoardNumberExistsAsync(string boardNumber, Guid? excludeOrderId = null)
        {
            if (string.IsNullOrWhiteSpace(boardNumber)) return false;
            var q = _db.Orders.AsQueryable().Where(o => o.BoardNumber == boardNumber);
            if (excludeOrderId.HasValue) q = q.Where(o => o.Id != excludeOrderId.Value);
            return await q.AnyAsync();
        }
    }
}
