using ElectronicsStore.Data;
using ElectronicsStore.ViewModels; // <-- Thêm using này
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicsStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tạo một instance của ViewModel
            var viewModel = new HomeViewModel
            {
                // Lấy 8 sản phẩm được đánh dấu nổi bật
                FeaturedProducts = await _context.Products
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync(),

                // Lấy 4 sản phẩm mới nhất
                NewArrivals = await _context.Products
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .ToListAsync(),

                // Lấy các danh mục để hiển thị
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            // Truyền toàn bộ viewModel vào View
            return View(viewModel);
        }
    }
}