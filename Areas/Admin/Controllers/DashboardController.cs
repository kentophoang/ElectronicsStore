using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.Where(o => o.IsPaid).SumAsync(o => o.TotalAmount); // Chỉ tính doanh thu từ đơn hàng đã thanh toán
            return View();
        }
        public async Task<IActionResult> ManageProducts()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
    }
}