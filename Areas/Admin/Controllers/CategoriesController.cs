using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization; // Thêm using này

namespace ElectronicsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                                           .Include(c => c.Parent)
                                           .OrderBy(c => c.ParentId)
                                           .ThenBy(c => c.Name)
                                           .ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateParentCategoriesDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ImageUrl,ParentId")] Category category)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                category.Slug = GenerateSlug(category.Name);
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo danh mục mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            await PopulateParentCategoriesDropDownList(category.ParentId);
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            await PopulateParentCategoriesDropDownList(category.ParentId, id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageUrl,ParentId")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (await _context.Categories.AnyAsync(c => c.Name == category.Name && c.Id != id))
            {
                ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Slug = GenerateSlug(category.Name);
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateParentCategoriesDropDownList(category.ParentId, id);
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories
                                        .Include(c => c.SubCategories)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            if (category.SubCategories.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa danh mục '{category.Name}' vì nó đang chứa các nhãn hàng/danh mục con.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id) => _context.Categories.Any(e => e.Id == id);

        // === CẢI TIẾN 1: TÁI SỬ DỤNG CODE TẢI DANH MỤC CHA ===
        private async Task PopulateParentCategoriesDropDownList(object selectedParent = null, int? excludeCategoryId = null)
        {
            var categoriesQuery = _context.Categories.Where(c => c.ParentId == null);
            if (excludeCategoryId != null)
            {
                categoriesQuery = categoriesQuery.Where(c => c.Id != excludeCategoryId.Value);
            }
            ViewBag.ParentCategories = new SelectList(await categoriesQuery.AsNoTracking().ToListAsync(), "Id", "Name", selectedParent);
        }

        // === CẢI TIẾN 2: HÀM TẠO SLUG GỌN VÀ MẠNH MẼ HƠN ===
        private static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return string.Empty;

            var s = phrase.ToLowerInvariant();

            // Bỏ dấu bằng cách chuẩn hóa Unicode
            s = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            s = sb.ToString().Normalize(NormalizationForm.FormC);

            s = Regex.Replace(s, @"\s+", "-"); // Thay khoảng trắng bằng gạch nối
            s = Regex.Replace(s, @"[^\w\-]", ""); // Xóa các ký tự không hợp lệ
            s = Regex.Replace(s, @"-{2,}", "-"); // Thay nhiều gạch nối bằng 1
            s = s.Trim('-');
            s = s.Substring(0, s.Length <= 45 ? s.Length : 45).Trim('-');
            return s;
        }
    }
}