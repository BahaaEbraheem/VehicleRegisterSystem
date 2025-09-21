using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using VehicleRegisterSystem.Application.DTOs;
using VehicleRegisterSystem.Application.DTOs.AuthenticationDTOs;
using VehicleRegisterSystem.Application.Interfaces;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Enums;

namespace VehicleRegisterSystem.Web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _userService = userService;
            _userManager = userManager;
        }

        // GET: Users

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var allUsers = await _userService.GetAllUsersAsync();
            var usersList = new List<UserViewModel>();

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                usersList.Add(new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    MembershipDate = u.LockoutEnd?.UtcDateTime, // أو تاريخ آخر تسجيل دخول
                    IsActive = !u.LockoutEnabled,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            var pagedUsers = usersList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PagedResult<UserViewModel>
            {
                Items = pagedUsers,
                TotalCount = usersList.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(model);
        }


        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            return View(new RegisterDto());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = $"{model.FirstName} {model.LastName}",
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Role = model.Role,
                EmailConfirmed = true
            };

            var result = await _userService.AddUserAsync(user, model.Password);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
                return View(model);
            }

            TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح - User created successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var userResult = await _userService.GetUserByIdAsync(id);
            if (!userResult.IsSuccess) return NotFound();

            var user = userResult.Data!;
            var names = user.FullName.Split(' ');
            var model = new EditUserDto
            {
                Id = user.Id,
                FirstName = names.First(),
                LastName = names.Length > 1 ? names[1] : "",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty, // not nullable
                Role = (UserRole)user.Role
                // Password and ConfirmPassword stay empty on GET
            };

            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            return View(model);
        }

        // POST: Users/Edit/{id}
        // POST: Users/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
                return View(model);
            }

            // Map EditUserDto → RegisterDto
            var registerDto = new RegisterDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber,
                Password = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password,
                ConfirmPassword = string.IsNullOrWhiteSpace(model.ConfirmPassword) ? null : model.ConfirmPassword,
                Role = model.Role
            };

            var updateResult = await _userService.UpdateUserAsync(model.Id, registerDto);

            if (!updateResult.IsSuccess)
            {
                ModelState.AddModelError("", updateResult.ErrorMessage);
                ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
                return View(model);
            }

            TempData["SuccessMessage"] = "تم تعديل المستخدم بنجاح - User updated successfully";
            return RedirectToAction(nameof(Index));
        }




        // POST: Users/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);

                if (result.IsSuccess)
                    TempData["SuccessMessage"] = "تم حذف المستخدم بنجاح!";
                else
                    TempData["ErrorMessage"] = result.ErrorMessage;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المستخدم";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Users/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var userResult = await _userService.GetUserByIdAsync(id);
            if (!userResult.IsSuccess) return NotFound();

            var user = userResult.Data!;
            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                MembershipDate = user.LockoutEnd?.UtcDateTime,
                IsActive = !user.LockoutEnabled,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }
    }
}
