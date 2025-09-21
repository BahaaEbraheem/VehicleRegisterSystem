using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Application.Services;

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
        public async Task<IActionResult> Index()
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
            if (!ModelState.IsValid)
            {
                // جمع جميع أخطاء التحقق
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                // تسجيل الأخطاء في Debug Output
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine(error);
                }

                // مؤقت: عرض الأخطاء في TempData (للتجربة فقط)
                TempData["ModelErrors"] = string.Join(" | ", errors);

                return View(dto);
            }

            var result = await _service.CreateAsync(dto, UserId, UserName);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    foreach (var err in result.ValidationErrors)
                        ModelState.AddModelError(string.Empty, err);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "حدث خطأ غير معروف");
                }
                return View(dto);
            }

            TempData["Message"] = "تم إنشاء الطلب بنجاح";
            return RedirectToAction("Index");
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

            try
            {
                await _service.UpdateAsync(id, dto, UserId, UserName);
                TempData["SuccessMessage"] = "تم تحديث الطلب بنجاح"; // ← رسالة النجاح
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء التحديث: {ex.Message}";
                return View(dto);
            }
        }


        // حذف الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id, UserId, UserName);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "تم حذف الطلب بنجاح";
            }
            else if (result.ValidationErrors?.Any() == true)
            {
                TempData["ErrorMessage"] = string.Join(" | ", result.ValidationErrors);
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "حدث خطأ أثناء الحذف";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid id)
        {
            var result = await _service.SubmitOrderAsync(id, UserId, UserName);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "حدث خطأ أثناء تقديم الطلب";
            }
            else
            {
                TempData["Message"] = "تم تقديم الطلب للمدقق بنجاح";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
