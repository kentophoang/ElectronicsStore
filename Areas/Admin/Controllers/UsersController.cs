using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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

        // <<< NÂNG CẤP ACTION INDEX VỚI TÌM KIẾM VÀ PHÂN TRANG >>>
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

        // Action này đã ổn, nhưng chúng ta sẽ tập trung vào EditRoles
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            return View(user);
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
                return View(model); // Trở lại view với lỗi
            }

            if (model.UserRoles != null && model.UserRoles.Any())
            {
                result = await _userManager.AddToRolesAsync(user, model.UserRoles);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Không thể thêm các vai trò đã chọn cho người dùng.");
                    return View(model); // Trở lại view với lỗi
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

            // <<< THÊM BẢO MẬT: Không cho admin tự xóa chính mình >>>
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                ModelState.AddModelError(string.Empty, "Bạn không thể xóa chính tài khoản của mình.");
                return View("Delete", user); // Trở lại trang Delete với thông báo lỗi
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa người dùng. Có thể người dùng này đã có đơn hàng hoặc dữ liệu liên quan.");
                return View("Delete", user); // Trở lại trang Delete với thông báo lỗi
            }

            return RedirectToAction(nameof(Index));
        }

        // <<< DỌN DẸP: Xóa Action Manage() không cần thiết >>>
    }
}