using Microsoft.AspNetCore.Authorization;
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
                // Save JWT in a HttpOnly cookie
                Response.Cookies.Append("AuthToken", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // set true in production
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }


        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterDto();
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
                // Redirect to login page after successful registration
                return RedirectToAction("Login");
            }

            // Add single error message if present
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }

            // Add multiple validation errors if present
            if (result.ValidationErrors?.Any() == true)
            {
                foreach (var error in result.ValidationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return View(model);
        }


        // GET: /Account/LogoutConfirmation
        [HttpGet]
        public IActionResult LogoutConfirmation()
        {
            var model = new LogoutDto
            {
                CurrentUserName = User.Identity?.Name ?? "غير معروف - Unknown",
                CurrentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? ""
            };
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Delete the JWT cookie
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1) // expire immediately
                });
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
