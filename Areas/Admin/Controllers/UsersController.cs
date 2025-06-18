using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using ElectronicsStore.ViewModels; // <-- THÊM DÒNG NÀY ĐỂ SỬ DỤNG CreateUserViewModel

namespace ElectronicsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Action Index với tìm kiếm và phân trang
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10;
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString) || u.FullName.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            usersQuery = usersQuery.OrderBy(u => u.Email);

            var totalUsers = await usersQuery.CountAsync();
            var users = await usersQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(users);
        }

        // << THÊM MỚI: CÁC ACTION ĐỂ TẠO NGƯỜI DÙNG >>
        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true // Tự động xác thực email khi admin tạo
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Mặc định gán vai trò "User"
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Nếu có lỗi, trả lại form với dữ liệu đã nhập
            return View(model);
        }


        // GET: Admin/Users/EditRoles/USER_ID
        public async Task<IActionResult> EditRoles(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AllRoles = allRoles.Select(r => r.Name).ToList(),
                UserRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: Admin/Users/EditRoles/USER_ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa các vai trò hiện tại của người dùng.");
                return View(model);
            }

            if (model.UserRoles != null && model.UserRoles.Any())
            {
                result = await _userManager.AddToRolesAsync(user, model.UserRoles);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Không thể thêm các vai trò đã chọn cho người dùng.");
                    return View(model);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            // Bảo mật: Không cho admin tự xóa chính mình
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                ModelState.AddModelError(string.Empty, "Bạn không thể xóa chính tài khoản của mình.");
                return View("Delete", user);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa người dùng. Có thể người dùng này đã có đơn hàng hoặc dữ liệu liên quan.");
                return View("Delete", user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}