using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Web.GlobalExceptionFiltersl;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize] // السماح فقط للمستخدمين المسجلين
    [TypeFilter(typeof(GlobalExceptionFilter))] // التعامل مع الأخطاء العامة
    public class OrdersController : Controller
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service) => _service = service;

        // معرف واسم المستخدم الحالي
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";

        #region عرض الطلبات

        /// <summary>
        /// عرض كل الطلبات الخاصة بالمستخدم الحالي
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _service.GetForUserAsync(UserId);
                return View(orders); // صفحة الطلبات MyOrders.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء عرض الطلبات: {ex.Message}";
                return View(Enumerable.Empty<OrderDto>());
            }
        }

        #endregion

        #region إنشاء طلب جديد

        /// <summary>
        /// GET: صفحة إنشاء طلب جديد
        /// </summary>
        [HttpGet]
        public IActionResult Create() => View();

        /// <summary>
        /// POST: إنشاء طلب جديد
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                // جمع جميع أخطاء التحقق وعرضها
                TempData["ErrorMessage"] = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return View(dto);
            }

            try
            {
                var result = await _service.CreateAsync(dto, UserId, UserName);

                if (!result.IsSuccess)
                {
                    if (result.ValidationErrors?.Any() == true)
                        foreach (var err in result.ValidationErrors)
                            ModelState.AddModelError(string.Empty, err);
                    else
                        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "حدث خطأ غير معروف");

                    return View(dto);
                }

                TempData["SuccessMessage"] = "تم إنشاء الطلب بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء إنشاء الطلب: {ex.Message}";
                return View(dto);
            }
        }

        #endregion

        #region تعديل الطلب

        /// <summary>
        /// GET: صفحة تعديل الطلب
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var order = await _service.GetByIdAsync(id);

                // التحقق من حالة الطلب
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تحميل بيانات الطلب: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: تعديل الطلب
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateOrderDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.UpdateAsync(id, dto, UserId, UserName);
                TempData["SuccessMessage"] = "تم تحديث الطلب بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء التحديث: {ex.Message}";
                return View(dto);
            }
        }

        #endregion

        #region حذف الطلب

        /// <summary>
        /// POST: حذف الطلب
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
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
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء حذف الطلب: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region تقديم الطلب

        /// <summary>
        /// POST: تقديم الطلب للمدقق
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid id)
        {
            try
            {
                var result = await _service.SubmitOrderAsync(id, UserId, UserName);

                if (result.IsSuccess)
                    TempData["SuccessMessage"] = "تم تقديم الطلب للمدقق بنجاح";
                else
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "حدث خطأ أثناء تقديم الطلب";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تقديم الطلب: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
