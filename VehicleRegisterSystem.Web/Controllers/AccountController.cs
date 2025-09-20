using Microsoft.AspNetCore.Mvc;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;

namespace VehicleRegisterSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;

        public AccountController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model);
            if (result.IsSuccess)
            {
                // Save JWT token or user info in session/cookie
                HttpContext.Session.SetString("Token", result.Data.Token);
                HttpContext.Session.SetString("FullName", result.Data.FullName);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.RegisterAsync(model);
            if (result.IsSuccess)
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }

        // GET: /Account/LogoutConfirmation
        [HttpGet]
        public IActionResult LogoutConfirmation()
        {
            var model = new LogoutDto
            {
                CurrentUserName = HttpContext.Session.GetString("FullName") ?? "غير معروف - Unknown",
                CurrentUserEmail = HttpContext.Session.GetString("Email") ?? ""
            };
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
