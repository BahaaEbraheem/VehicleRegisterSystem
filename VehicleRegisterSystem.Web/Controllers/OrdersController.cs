using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _service;
        public OrdersController(IOrderService service) => _service = service;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";

        // عرض كل الطلبات للمستخدم الحالي
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _service.GetForUserAsync(UserId);
            return View(orders); // MyOrders.cshtml
        }

        // إنشاء طلب جديد — GET
        [HttpGet]
        public IActionResult Create() => View();

        // إنشاء طلب جديد — POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _service.CreateAsync(dto, UserId, UserName);
            return RedirectToAction(nameof(MyOrders));
        }

        // تعديل الطلب — GET
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order.Status != Domain.Enums.OrderStatus.New && order.Status != Domain.Enums.OrderStatus.Returned)
                return Forbid();

            var dto = new UpdateOrderDto
            {
                FullName = order.FullName,
                NationalNumber = order.NationalNumber,
                MotherName = order.MotherName,
                CarName = order.CarName,
                Model = order.Model,
                YearOfManufacture = order.YearOfManufacture,
                Color = order.Color,
                EngineNumber = order.EngineNumber
            };
            return View(dto);
        }

        // تعديل الطلب — POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateOrderDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _service.UpdateAsync(id, dto, UserId, UserName);
            return RedirectToAction(nameof(MyOrders));
        }

        // حذف الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id, UserId, UserName);
            return RedirectToAction(nameof(MyOrders));
        }

    }
}
