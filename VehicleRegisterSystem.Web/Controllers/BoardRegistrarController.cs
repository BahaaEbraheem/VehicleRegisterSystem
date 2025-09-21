using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize(Roles = "BoardRegistrar,Administrator")]
    public class BoardRegistrarController : Controller
    {
        private readonly IOrderService _orderService;
        public BoardRegistrarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";

        // عرض كل الطلبات في الحالة "قيد الإجراء"
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetByStatusesAsync(OrderStatus.InProgress);
            return View(orders); // Views/BoardRegistrar/Index.cshtml
        }

        // GET: تسجيل لوحة السيارة
        [HttpGet]
        public async Task<IActionResult> RegisterBoard(Guid? id)
        {
            RegisterBoardDto dto = null;

            if (id.HasValue)
            {
                var order = await _orderService.GetByIdAsync(id.Value);
                if (order != null && order.Status == OrderStatus.InProgress)
                {
                    dto = new RegisterBoardDto
                    {
                        OrderId = id.Value,
                        CarName = order.CarName,
                        Model = order.Model,
                        EngineNumber = order.EngineNumber
                    };
                }
            }

            // إذا لم يوجد طلب صالح، نمرر DTO فارغ
            if (dto == null)
            {
                dto = new RegisterBoardDto();
                ViewData["DisableForm"] = true; // لتعطيل الحقول والأزرار
            }

            return View(dto);
        }


        // POST: تسجيل لوحة السيارة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBoard(RegisterBoardDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // استدعاء الخدمة
            var result = await _orderService.RegisterBoardAsync(
                dto.OrderId,
                dto.BoardNumber,
                User.Identity.Name, // أو أي معرف المستخدم
                User.Identity.Name
            );

            if (!result.IsSuccess)
            {
                // إذا كانت هناك ValidationErrors
                if (result.ValidationErrors.Any())
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "حدث خطأ غير معروف.");
                }
                return View(dto);
            }

            TempData["Message"] = "تم تسجيل لوحة السيارة بنجاح!";
            return RedirectToAction(nameof(RegisterBoard), new { id = dto.OrderId });
        }
        [Authorize(Roles = "BoardRegistrar,Administrator")]
        public async Task<IActionResult> ApprovedOrders()
        {
            // جلب كل الطلبات التي تم تسجيلها بنجاح
            var orders = await _orderService.GetByStatusesAsync(OrderStatus.Approved);
            return View(orders); // Views/BoardRegistrar/ApprovedOrders.cshtml
        }

    }
}
