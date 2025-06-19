using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicsStore.Areas.Admin.Controllers
{
    // Quan trọng: Phải có Attribute này để hệ thống biết Controller thuộc Area "Admin"
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    // Quan trọng: Tên class phải là "OrdersController"
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action mặc định (Index) sẽ được gọi khi truy cập /Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                                     .Include(o => o.User)
                                     .OrderByDescending(o => o.OrderDate)
                                     .ToListAsync();
            return View(orders);
        }

        // Action Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Action UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}