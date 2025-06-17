using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using ElectronicsStore.Models;
using ElectronicsStore.ViewModels; // <<< THÊM USING NÀY >>>

namespace ElectronicsStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 12; // Định nghĩa kích thước trang ở một nơi

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action INDEX đã được làm lại hoàn toàn để dùng ViewModel
        public async Task<IActionResult> Index(int? categoryId, string searchString, int pageNumber = 1)
        {
            // 1. Bắt đầu với một IQueryable<Product> cơ bản
            var productsQuery = _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            string currentCategoryName = null;

            // 2. Lọc theo danh mục nếu có
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                var category = await _context.Categories.FindAsync(categoryId.Value);
                if (category != null)
                {
                    currentCategoryName = category.Name;
                }
            }

            // 3. Lọc theo chuỗi tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.ShortDescription.Contains(searchString));
            }

            // 4. Lấy tổng số sản phẩm TRƯỚC KHI phân trang
            var totalProducts = await productsQuery.CountAsync();

            // 5. Thực hiện phân trang
            var paginatedProducts = await productsQuery
                .OrderByDescending(p => p.Id) // Sắp xếp theo sản phẩm mới nhất
                .Skip((pageNumber - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // 6. Lấy toàn bộ danh sách danh mục cho menu
            var allCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            // 7. Tạo ViewModel và đóng gói TẤT CẢ dữ liệu cần thiết
            var viewModel = new ProductListViewModel
            {
                Products = paginatedProducts,
                Categories = allCategories,
                CurrentCategoryId = categoryId,
                CurrentCategoryName = currentCategoryName,
                SearchString = searchString,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalProducts / _pageSize)
            };

            // 8. Trả về View với một model duy nhất, tường minh
            return View(viewModel);
        }

        // Action Details giữ nguyên, nó đã tốt rồi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // <<< ACTION SEARCH ĐÃ BỊ XÓA HOÀN TOÀN >>>
        // Nó không cần thiết nữa vì mọi chức năng đã được tích hợp trong Index.
    }
}