using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ElectronicsStore.Data;
using ElectronicsStore.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace ElectronicsStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                                        .Where(o => o.UserId == user.Id)
                                        .Include(o => o.OrderItems)
                                            .ThenInclude(oi => oi.Product)
                                        .OrderByDescending(o => o.OrderDate)
                                        .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var order = await _context.Orders
                                      .Include(o => o.OrderItems)
                                          .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([FromForm] CheckoutViewModel model)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = session?.GetString("Cart");

            List<CartItem> cart = (cartJson != null)
                ? JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>()
                : new List<CartItem>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("Checkout", "Cart") });
            }

            if (!ModelState.IsValid)
            {
                var checkoutVm = new CheckoutPageViewModel
                {
                    CartItems = cart,
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ShippingAddress = model.ShippingAddress,
                    PaymentMethod = model.PaymentMethod
                };
                TempData["ErrorMessage"] = "Thông tin đặt hàng không hợp lệ. Vui lòng kiểm tra lại.";
                return View("Checkout", checkoutVm);
            }

            decimal totalAmountFromServer = 0;
            var orderItems = new List<OrderItem>();
            var productIdsInCart = cart.Select(c => c.ProductId).ToList();
            var productsInDb = await _context.Products.Where(p => productIdsInCart.Contains(p.Id)).ToListAsync();

            foreach (var item in cart)
            {
                var productInDb = productsInDb.FirstOrDefault(p => p.Id == item.ProductId);
                if (productInDb == null)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' không còn tồn tại.";
                    return RedirectToAction("Index", "Cart");
                }

                if (productInDb.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Số lượng tồn kho của sản phẩm '{productInDb.Name}' không đủ. Chỉ còn lại {productInDb.StockQuantity}.";
                    return RedirectToAction("Index", "Cart");
                }

                productInDb.StockQuantity -= item.Quantity;

                totalAmountFromServer += productInDb.Price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = productInDb.Price
                });
            }

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                ShippingAddress = model.ShippingAddress,
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email ?? user.Email,
                TotalAmount = totalAmountFromServer,
                Status = OrderStatus.Pending,
                PaymentMethod = model.PaymentMethod,
                IsPaid = (model.PaymentMethod != "COD"),
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            session?.Remove("Cart");

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var order = await _context.Orders
                                      .Include(o => o.OrderItems)
                                          .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}