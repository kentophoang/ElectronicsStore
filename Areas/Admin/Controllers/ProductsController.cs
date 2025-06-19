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
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace ElectronicsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = _context.Products.Include(p => p.Category).OrderByDescending(p => p.CreatedDate);
            return View(await products.ToListAsync());
        }

        // GET: Admin/Products/Create (Đã cập nhật)
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownList();
            return View();
        }

        // POST: Admin/Products/Create (Đã cập nhật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            // Bỏ qua lỗi validation của trường MainImageUrl vì ta sẽ gán nó sau khi tải file
            ModelState.Remove("MainImageUrl");

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    product.MainImageUrl = await UploadImage(imageFile);
                }

                product.Slug = GenerateSlug(product.Name);
                product.CreatedDate = DateTime.Now;
                product.UpdatedDate = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropDownList(product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5 (Đã cập nhật)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            await PopulateCategoriesDropDownList(product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5 (Đã cập nhật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("MainImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var productToUpdate = await _context.Products.FindAsync(id);
                    if (productToUpdate == null) return NotFound();

                    if (imageFile != null)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(productToUpdate.MainImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToUpdate.MainImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        productToUpdate.MainImageUrl = await UploadImage(imageFile);
                    }

                    productToUpdate.Name = product.Name;
                    productToUpdate.ShortDescription = product.ShortDescription;
                    productToUpdate.FullDescription = product.FullDescription;
                    productToUpdate.Price = product.Price;
                    productToUpdate.OriginalPrice = product.OriginalPrice;
                    productToUpdate.StockQuantity = product.StockQuantity;
                    productToUpdate.CategoryId = product.CategoryId;
                    productToUpdate.IsActive = product.IsActive;
                    productToUpdate.IsFeatured = product.IsFeatured;
                    productToUpdate.Slug = GenerateSlug(product.Name); // Cập nhật slug khi tên thay đổi
                    productToUpdate.UpdatedDate = DateTime.Now;

                    _context.Update(productToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoriesDropDownList(product.CategoryId);
            return View(product);
        }

        // ... Các action Delete và các phương thức khác ...
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.MainImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.MainImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id) => _context.Products.Any(e => e.Id == id);

        // === CẢI TIẾN: PHƯƠNG THỨC TÁI SỬ DỤNG ĐỂ TẢI DANH SÁCH DANH MỤC/NHÃN HÀNG ===
        private async Task PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categoriesQuery = _context.Categories
                                          .Where(c => c.ParentId != null) // Chỉ lấy các danh mục con (nhãn hàng)
                                          .Include(c => c.Parent)
                                          .OrderBy(c => c.Parent.Name)
                                          .ThenBy(c => c.Name);

            // Tạo SelectList có phân nhóm theo tên danh mục cha
            ViewData["CategoryId"] = new SelectList(await categoriesQuery.AsNoTracking().ToListAsync(), "Id", "Name", selectedCategory, "Parent.Name");
        }

        // Phương thức tải ảnh
        private async Task<string> UploadImage(IFormFile imageFile)
        {
            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(uploadsDir, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/images/products/" + uniqueFileName;
        }

        // Phương thức tạo slug
        private static string GenerateSlug(string phrase)
        {
            // ... (giữ nguyên hàm GenerateSlug của bạn) ...
            if (string.IsNullOrEmpty(phrase)) return string.Empty;
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}