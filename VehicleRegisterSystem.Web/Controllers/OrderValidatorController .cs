using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain.Enums;
using VehicleRegisterSystem.Web.GlobalExceptionFiltersl;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize(Roles = "OrderValidator,Administrator")]
    [TypeFilter(typeof(GlobalExceptionFilter))] // التعامل مع الأخطاء العامة

    public class OrderValidatorController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderValidatorController(IOrderService orderService) => _orderService = orderService;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";

        #region عرض الطلبات

        /// <summary>
        /// عرض الطلبات الجديدة والمعادة والمعدلة للمراجعة
        /// </summary>
        public async Task<IActionResult> PendingValidation()
        {
            try
            {
                var orders = await _orderService.GetNewAndReturnedAndModifiedOrdersAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء جلب الطلبات: {ex.Message}";
                return View(Array.Empty<OrderDto>());
            }
        }

        /// <summary>
        /// عرض الطلبات التي قيد الإجراء (يمكن للمدقق فقط الاطلاع)
        /// </summary>
        public async Task<IActionResult> InReview()
        {
            try
            {
                var orders = await _orderService.GetByStatusesAsync(OrderStatus.InProgress);
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء جلب الطلبات قيد الإجراء: {ex.Message}";
                return View(Array.Empty<OrderDto>());
            }
        }

        #endregion

        #region التحقق من الطلبات

        /// <summary>
        /// GET: عرض تفاصيل الطلب للتحقق منه
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ValidateOrder(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null) return NotFound();
                if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned) return Forbid();

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تحميل بيانات الطلب: {ex.Message}";
                return RedirectToAction("PendingValidation");
            }
        }

        /// <summary>
        /// POST: إعادة الطلب إلى المستخدم
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnOrder(OrderDto dto)
        {
            try
            {
                var result = await _orderService.ReturnToUserAsync(dto.Id, UserId, UserName, dto.CurrentReturnComment);

                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.ErrorMessage ?? "حدث خطأ أثناء إعادة الطلب");
                    var order = await _orderService.GetByIdAsync(dto.Id);
                    return View("ValidateOrder", order);
                }

                TempData["Message"] = "تم إعادة الطلب للمستخدم";
                return RedirectToAction("PendingValidation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء إعادة الطلب: {ex.Message}";
                return RedirectToAction("ValidateOrder", new { id = dto.Id });
            }
        }

        /// <summary>
        /// POST: وضع الطلب قيد الإجراء
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetInProgress(Guid id)
        {
            try
            {
                var result = await _orderService.SetInProgressAsync(id, UserId, UserName);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "حدث خطأ أثناء نقل الطلب إلى قيد الإجراء";
                    return RedirectToAction("ValidateOrder", new { id });
                }

                TempData["Message"] = "تم نقل الطلب إلى قيد الإجراء";
                return RedirectToAction("PendingValidation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء نقل الطلب إلى قيد الإجراء: {ex.Message}";
                return RedirectToAction("ValidateOrder", new { id });
            }
        }

        #endregion
    }
}
