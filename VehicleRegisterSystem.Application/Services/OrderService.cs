using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Validation;
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

        #region إنشاء الطلب
        public async Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, string userId, string userName)
        {
            try
            {
                // التحقق من رقم المحرك المكرر
                if (await _repo.EngineNumberExistsAsync(dto.EngineNumber))
                    return ServiceResult<OrderDto>.Failure("رقم المحرك مكرر. الرجاء إدخال رقم آخر.", "DUPLICATE_ENGINE");

                // التحقق من البيانات الأساسية
                var validationErrors = new List<string>();
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    validationErrors.Add("اسم مقدم الطلب مطلوب.");
                if (string.IsNullOrWhiteSpace(dto.NationalNumber))
                    validationErrors.Add("الرقم الوطني مطلوب.");
                if (string.IsNullOrWhiteSpace(dto.CarName))
                    validationErrors.Add("اسم السيارة مطلوب.");
                if (string.IsNullOrWhiteSpace(dto.Model))
                    validationErrors.Add("الموديل مطلوب.");
                if (dto.YearOfManufacture <= 0)
                    validationErrors.Add("سنة الصنع غير صحيحة.");

                if (validationErrors.Any())
                    return ServiceResult<OrderDto>.ValidationFailure(validationErrors);

                // إنشاء الطلب
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
                    Status = OrderStatus.Draft
                };

                await _repo.AddAsync(order);
                _cache.Remove($"user_orders_{userId}");

                return ServiceResult<OrderDto>.Success(Map(order));
            }
            catch (Exception ex)
            {
                return ServiceResult<OrderDto>.Failure($"حدث خطأ أثناء إنشاء الطلب: {ex.Message}");
            }
        }
        #endregion

        #region حذف الطلب
        public async Task<ServiceResult<bool>> DeleteAsync(Guid id, string userId, string userName)
        {
            try
            {
                var order = await _repo.GetByIdAsync(id);
                if (order == null)
                    return ServiceResult<bool>.Failure("الطلب غير موجود");

                if (order.Status != OrderStatus.Draft)
                    return ServiceResult<bool>.ValidationFailure(new List<string> { "لا يمكن حذف الطلب إلا إذا كان مسودة" });

                order.IsDeleted = true;
                order.DeletedAt = DateTime.UtcNow;
                order.DeletedById = userId;
                order.DeletedByName = userName;

                await _repo.UpdateAsync(order);
                _cache.Remove($"order_{id}");
                _cache.Remove($"user_orders_{order.CreatedById}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"حدث خطأ أثناء حذف الطلب: {ex.Message}");
            }
        }
        #endregion

        #region جلب الطلبات
        public async Task<OrderDto> GetByIdAsync(Guid id)
        {
            try
            {
                var cacheKey = $"order_{id}";
                if (_cache.TryGetValue(cacheKey, out OrderDto cached)) return cached;

                var order = await _repo.GetByIdAsync(id);
                if (order == null) return null;

                var dto = Map(order);
                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(3));
                return dto;
            }
            catch
            {
                return null; // تجنب رمي استثناءات هنا
            }
        }

        public async Task<IEnumerable<OrderDto>> GetForUserAsync(string userId)
        {
            try
            {
                var cacheKey = $"user_orders_{userId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderDto> cached)) return cached;

                var list = await _repo.GetByUserAsync(userId);
                list = list.Where(a => a.Status == OrderStatus.Draft || a.Status == OrderStatus.Returned || a.Status == OrderStatus.Approved).ToList();

                var dtos = list.Select(Map).ToList();
                _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(2));
                return dtos;
            }
            catch
            {
                return Enumerable.Empty<OrderDto>();
            }
        }

        public async Task<IEnumerable<OrderDto>> GetNewAndReturnedAndModifiedOrdersAsync()
        {
            try
            {
                var orders = await _repo.GetNewAndReturnedAndModifiedOrdersAsync();
                return orders.Select(Map);
            }
            catch
            {
                return Enumerable.Empty<OrderDto>();
            }
        }

        public async Task<IEnumerable<OrderDto>> GetByStatusesAsync(OrderStatus[] statuses)
        {
            try
            {
                var orders = await _repo.GetByStatusesAsync(statuses);
                return orders.Select(Map);
            }
            catch
            {
                return Enumerable.Empty<OrderDto>();
            }
        }
        #endregion

        #region تعديل الطلب
        public async Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto dto, string userId, string userName)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء تعديل الطلب: {ex.Message}");
            }
        }
        #endregion

        #region تقديم الطلب وإعادة الطلب
        public async Task<ServiceResult<bool>> ReturnToUserAsync(Guid id, string validatorId, string validatorName, string comment)
        {
            try
            {
                var order = await _repo.GetByIdAsync(id);
                if (order == null)
                    return ServiceResult<bool>.Failure("الطلب غير موجود", "ORDER_NOT_FOUND");

                if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned)
                    return ServiceResult<bool>.Failure("لا يمكن إعادة الطلب من هذه الحالة", "INVALID_STATUS");

                if (string.IsNullOrWhiteSpace(comment))
                    return ServiceResult<bool>.Failure("سبب إعادة الطلب مطلوب", "MISSING_COMMENT");

                order.Status = OrderStatus.Returned;
                order.StatusChangedAt = DateTime.UtcNow;
                order.StatusChangedById = validatorId;
                order.StatusChangedByName = validatorName;
                order.ReturnComment = comment;

                await _repo.UpdateAsync(order);
                _cache.Remove($"order_{id}");
                _cache.Remove($"user_orders_{order.CreatedById}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"حدث خطأ أثناء إعادة الطلب: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SetInProgressAsync(Guid id, string validatorId, string validatorName)
        {
            try
            {
                var order = await _repo.GetByIdAsync(id);
                if (order == null)
                    return ServiceResult<bool>.Failure("الطلب غير موجود", "ORDER_NOT_FOUND");

                if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned)
                    return ServiceResult<bool>.Failure("لا يمكن نقل الطلب من الحالة الحالية إلى قيد الإجراء", "INVALID_STATUS");

                if (await _repo.EngineNumberExistsAsync(order.EngineNumber, order.Id))
                    return ServiceResult<bool>.Failure("رقم المحرك مكرر. الرجاء تصحيح البيانات.", "DUPLICATE_ENGINE");

                if (string.IsNullOrWhiteSpace(order.CarName) ||
                    string.IsNullOrWhiteSpace(order.Model) ||
                    order.YearOfManufacture <= 0)
                {
                    return ServiceResult<bool>.Failure("بيانات السيارة ناقصة. الرجاء تصحيح الطلب قبل التقدم.", "MISSING_DATA");
                }

                order.Status = OrderStatus.InProgress;
                order.StatusChangedAt = DateTime.UtcNow;
                order.StatusChangedById = validatorId;
                order.StatusChangedByName = validatorName;

                await _repo.UpdateAsync(order);
                _cache.Remove($"order_{id}");
                _cache.Remove($"user_orders_{order.CreatedById}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"حدث خطأ أثناء وضع الطلب قيد الإجراء: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SubmitOrderAsync(Guid orderId, string userId, string userName)
        {
            try
            {
                var order = await _repo.GetByIdAsync(orderId);
                if (order == null)
                    return ServiceResult<bool>.Failure("الطلب غير موجود", "ORDER_NOT_FOUND");

                if (order.Status != OrderStatus.Draft)
                    return ServiceResult<bool>.Failure("الطلب لا يمكن تقديمه", "INVALID_STATUS");

                order.Status = OrderStatus.New;
                order.StatusChangedAt = DateTime.UtcNow;
                order.StatusChangedById = userId;
                order.StatusChangedByName = userName;

                await _repo.UpdateAsync(order);
                _cache.Remove($"order_{orderId}");
                _cache.Remove($"user_orders_{order.CreatedById}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"حدث خطأ أثناء تقديم الطلب: {ex.Message}");
            }
        }
        #endregion

        #region تسجيل لوحة السيارة
        public async Task<ServiceResult<bool>> RegisterBoardAsync(Guid id, string boardNumber, string registrarId, string registrarName)
        {
            try
            {
                var order = await _repo.GetByIdAsync(id);
                if (order == null)
                    return ServiceResult<bool>.Failure("الطلب غير موجود", "ORDER_NOT_FOUND");

                if (order.Status != OrderStatus.InProgress)
                    return ServiceResult<bool>.Failure("الطلب ليس في حالة قيد الإجراء", "INVALID_STATUS");

                if (await _repo.BoardNumberExistsAsync(boardNumber, id))
                    return ServiceResult<bool>.Failure("رقم اللوحة موجود مسبقاً", "DUPLICATE_BOARD");

                boardNumber = boardNumber.ToUpper().Trim();
                var regex = new Regex("^(?=.*[A-Z])[A-Z0-9]+$");
                if (!regex.IsMatch(boardNumber))
                    return ServiceResult<bool>.Failure("رقم اللوحة يجب أن يحتوي فقط على حروف كبيرة وأرقام", "INVALID_FORMAT");

                order.BoardNumber = boardNumber;
                order.Status = OrderStatus.Approved;
                order.StatusChangedAt = DateTime.UtcNow;
                order.StatusChangedById = registrarId;
                order.StatusChangedByName = registrarName;

                await _repo.UpdateAsync(order);

                _cache.Remove($"order_{id}");
                _cache.Remove($"user_orders_{order.CreatedById}");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"حدث خطأ أثناء تسجيل لوحة السيارة: {ex.Message}");
            }
        }
        #endregion

        #region Mapping
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
            CurrentReturnComment = o.ReturnComment,
            BoardNumber = o.BoardNumber
        };
        #endregion
    }
}