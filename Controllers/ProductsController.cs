using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using ElectronicsStore.Models;
using ElectronicsStore.ViewModels;
using System.Collections.Generic; // Cần có using này cho Dictionary

namespace ElectronicsStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 12; // Kích thước trang

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string categorySlug, string searchString, int pageNumber = 1)
        {
            var productsQuery = _context.Products
                                        .Include(p => p.Category)
                                        .Where(p => p.IsActive);

            Category currentCategory = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                currentCategory = await _context.Categories
                                                .Include(c => c.SubCategories)
                                                .FirstOrDefaultAsync(c => c.Slug == categorySlug);

                if (currentCategory != null)
                {
                    var categoryIdsToFilter = new List<int> { currentCategory.Id };
                    if (currentCategory.SubCategories.Any())
                    {
                        categoryIdsToFilter.AddRange(currentCategory.SubCategories.Select(sc => sc.Id));
                    }
                    productsQuery = productsQuery.Where(p => categoryIdsToFilter.Contains(p.CategoryId));
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            var totalProducts = await productsQuery.CountAsync();

            var paginatedProducts = await productsQuery
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // === THÊM Ở ĐÂY: Lấy danh sách tất cả danh mục cho sidebar ===
            var allCategoriesForSidebar = await _context.Categories
                                                        .OrderBy(c => c.ParentId).ThenBy(c => c.Name)
                                                        .ToListAsync();

            // Tạo ViewModel và gán đầy đủ các giá trị
            var viewModel = new ProductListViewModel
            {
                Products = paginatedProducts,
                Categories = allCategoriesForSidebar,           // Gửi danh sách cho sidebar
                CurrentCategoryId = currentCategory?.Id,         // Gửi Id của mục đang active
                CurrentCategoryName = currentCategory?.Name,
                SearchString = searchString,
                PageNumber = pageNumber,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize),
                RouteData = new Dictionary<string, string>
                {
                    { "categorySlug", categorySlug },
                    { "searchString", searchString }
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}