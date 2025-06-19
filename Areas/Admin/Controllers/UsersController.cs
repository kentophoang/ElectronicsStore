using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Đảm bảo biến này được khai báo

        // Constructor này sẽ inject (tiêm vào) các dịch vụ cần thiết.
        // Dòng gán _context = context; là cực kỳ quan trọng.
        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context; // Lỗi phát sinh nếu dòng này bị thiếu
        }

        // Action Index hỗ trợ tìm kiếm và phân trang
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery
                    .Where(u => u.FullName.Contains(searchString) || u.Email.Contains(searchString));
            }

            usersQuery = usersQuery.OrderBy(u => u.FullName);

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var totalItems = await usersQuery.CountAsync();
            ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(users);
        }

        // Action xem lịch sử giao dịch của người dùng
        [HttpGet]
        public async Task<IActionResult> TransactionHistory(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Dòng này sử dụng _context, nay đã được khởi tạo đúng
            var orders = await _context.Orders
                               .Where(o => o.UserId == id)
                               .Include(o => o.OrderItems)
                               .ThenInclude(oi => oi.Product)
                               .OrderByDescending(o => o.OrderDate)
                               .ToListAsync();

            ViewBag.UserName = user.UserName;
            return View(orders);
        }

        // Các action khác được link từ View (tạm thời để tránh lỗi 404)
        public async Task<IActionResult> EditRoles(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserName = user.UserName;
            // TODO: Xây dựng logic và view để sửa vai trò
            return Content($"Chức năng sửa vai trò cho user: {user.UserName} đang được phát triển.");
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            // TODO: Xây dựng logic và view để xác nhận xóa
            return Content($"Chức năng xóa user: {user.UserName} đang được phát triển.");
        }
    }
}