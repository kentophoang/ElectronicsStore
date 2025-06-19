using ElectronicsStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicsStore.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy các danh mục cấp 1 và các danh mục con của chúng
            var categories = await _context.Categories
                                     .Where(c => c.ParentId == null)
                                     .Include(c => c.SubCategories)
                                     .OrderBy(c => c.Name)
                                     .ToListAsync();
            return View(categories);
        }
    }
}