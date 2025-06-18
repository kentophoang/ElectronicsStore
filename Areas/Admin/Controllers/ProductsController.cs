using ElectronicsStore.Data;
using ElectronicsStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 8;
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            productsQuery = productsQuery.OrderByDescending(p => p.CreatedDate);

            var totalProducts = await productsQuery.CountAsync();
            var products = await productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(products);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsDir, uniqueFileName);

                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    // SỬA LỖI TẠI ĐÂY:
                    product.MainImageUrl = "/images/products/" + uniqueFileName;
                }

                product.CreatedDate = DateTime.Now;
                product.UpdatedDate = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.Id) return NotFound();

            var productToUpdate = await _context.Products.FindAsync(id);
            if (productToUpdate == null) return NotFound();

            // Bỏ qua lỗi validation của trường MainImageUrl vì nó không được bind trực tiếp
            ModelState.Remove("MainImageUrl");

            if (ModelState.IsValid)
            {
                if (ImageFile != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsDir, uniqueFileName);

                    // SỬA LỖI TẠI ĐÂY:
                    if (!string.IsNullOrEmpty(productToUpdate.MainImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToUpdate.MainImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    // SỬA LỖI TẠI ĐÂY:
                    productToUpdate.MainImageUrl = "/images/products/" + uniqueFileName;
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
                productToUpdate.Slug = product.Slug;
                productToUpdate.UpdatedDate = DateTime.Now;

                try
                {
                    _context.Update(productToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

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
                // SỬA LỖI TẠI ĐÂY:
                if (!string.IsNullOrEmpty(product.MainImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.MainImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _context.Products.Remove(product);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}