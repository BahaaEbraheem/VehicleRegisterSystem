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
    [Authorize(Roles = "BoardRegistrar,Administrator")]
    [TypeFilter(typeof(GlobalExceptionFilter))] // التعامل مع الأخطاء العامة

    public class BoardRegistrarController : Controller
    {
        private readonly IOrderService _orderService;

        public BoardRegistrarController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string UserName => User.Identity?.Name ?? "";

        #region عرض الطلبات

        /// <summary>
        /// عرض كل الطلبات في الحالة "قيد الإجراء"
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetByStatusesAsync(OrderStatus.InProgress);
                return View(orders); // Views/BoardRegistrar/Index.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء جلب الطلبات: {ex.Message}";
                return View(Array.Empty<OrderDto>());
            }
        }

        /// <summary>
        /// عرض الطلبات التي تم اعتمادها
        /// </summary>
        public async Task<IActionResult> ApprovedOrders()
        {
            try
            {
                var orders = await _orderService.GetByStatusesAsync(OrderStatus.Approved);
                return View(orders); // Views/BoardRegistrar/ApprovedOrders.cshtml
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء جلب الطلبات المعتمدة: {ex.Message}";
                return View(Array.Empty<OrderDto>());
            }
        }

        #endregion

        #region تسجيل لوحة السيارة

        /// <summary>
        /// GET: عرض نموذج تسجيل لوحة السيارة
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RegisterBoard(Guid? id)
        {
            try
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

                if (dto == null)
                {
                    dto = new RegisterBoardDto();
                    ViewData["DisableForm"] = true; // لتعطيل الحقول والأزرار
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تحميل بيانات الطلب: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: تسجيل لوحة السيارة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBoard(RegisterBoardDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var result = await _orderService.RegisterBoardAsync(
                    dto.OrderId,
                    dto.BoardNumber,
                    UserId,
                    UserName
                );

                if (!result.IsSuccess)
                {
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء تسجيل لوحة السيارة: {ex.Message}";
                return View(dto);
            }
        }

        #endregion
    }
}
