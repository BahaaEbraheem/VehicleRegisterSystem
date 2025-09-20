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
    [Authorize(Roles = "BoardRegistrar,Admin")]
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
            var orders = await _orderService.GetByStatusAsync(OrderStatus.InProgress);
            return View(orders); // Views/BoardRegistrar/Index.cshtml
        }

        // GET: تسجيل لوحة السيارة
        [HttpGet]
        public async Task<IActionResult> RegisterBoard(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order.Status != OrderStatus.InProgress)
                return Forbid();

            var dto = new RegisterBoardDto
            {
                OrderId = id,
                CarName = order.CarName,
                Model = order.Model,
                EngineNumber = order.EngineNumber
            };
            return View(dto); // Views/BoardRegistrar/RegisterBoard.cshtml
        }

        // POST: تسجيل لوحة السيارة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBoard(RegisterBoardDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var success = await _orderService.RegisterBoardAsync(dto.OrderId, dto.BoardNumber, UserId, UserName);
                if (success)
                {
                    TempData["Message"] = "Board registered successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Board registration failed. Number might exist or order not in progress.");
                    return View(dto);
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }
    }
}
