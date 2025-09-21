using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize(Roles = "OrderValidator,Administrator")]
    public class OrderValidatorController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderValidatorController(IOrderService orderService) => _orderService = orderService;

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";


        public async Task<IActionResult> PendingValidation()
        {
            var orders = await _orderService.GetNewAndReturnedAndModifiedOrdersAsync();
            return View(orders); // Views/OrderValidator/PendingValidation.cshtml
        }
        // الطلبات قيد الإجراء (يمكن للمدقق فقط الاطلاع، لا تعديل)
        public async Task<IActionResult> InReview()
        {
            var orders = await _orderService.GetByStatusesAsync(OrderStatus.InProgress);
            return View(orders); // Views/OrderValidator/InReview.cshtml
        }

        // GET: عرض تفاصيل الطلب
        [HttpGet]
        public async Task<IActionResult> ValidateOrder(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();
            if (order.Status != OrderStatus.New && order.Status != OrderStatus.Returned)
                return Forbid();

            return View(order); // تفاصيل الطلب مع أزرار "إعادة الطلب" و "قيد الإجراء"
        }

        // POST: إعادة الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnOrder(OrderDto dto)
        {
            var result = await _orderService.ReturnToUserAsync(
                dto.Id,
                UserId,
                UserName,
                dto.CurrentReturnComment
            );

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "حدث خطأ أثناء إعادة الطلب");
                var order = await _orderService.GetByIdAsync(dto.Id);
                return View("ValidateOrder", order);
            }

            TempData["Message"] = "تم إعادة الطلب للمستخدم";
            return RedirectToAction("PendingValidation");
        }

        // POST: وضع الطلب قيد الإجراء
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetInProgress(Guid id)
        {
            var result = await _orderService.SetInProgressAsync(id, UserId, UserName);

            if (!result.IsSuccess)
            {
                // طريقة 1: عرض الخطأ برسالة مؤقتة (TempData)
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("ValidateOrder", new { id }); // رجوع لنفس صفحة التدقيق

                // طريقة 2 (أفضل): استخدام ModelState
                // ModelState.AddModelError("", result.ErrorMessage ?? "حدث خطأ غير متوقع");
                // var order = await _orderService.GetOrderDtoByIdAsync(id);
                // return View("ValidateOrder", order);
            }

            TempData["Message"] = "تم نقل الطلب إلى قيد الإجراء";
            return RedirectToAction("PendingValidation");
        }
    }


}
