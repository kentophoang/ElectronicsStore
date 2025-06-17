using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using ElectronicsStore.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Models; // <<< THÊM DÒNG NÀY VÀO ĐÂY >>>
using Microsoft.AspNetCore.Identity; // <<< CẦN CHO Action Checkout nếu có sử dụng _userManager >>>

namespace ElectronicsStore.Controllers // <<< THÊM DÒNG NÀY VÀO ĐÂY >>>
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // <<< Cần nếu dùng trong Checkout() >>>

        // Cần thêm UserManager vào constructor nếu bạn sử dụng nó trong action Checkout()
        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Gán UserManager
        }

        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson == null ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson)!;
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem // <<< Đã sửa lỗi nếu nó vẫn còn ở đây >>>
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.MainImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng.", cartItemCount = cart.Sum(i => i.Quantity) });
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCartToSession(cart);
                return Json(new { success = true, newTotal = item.Total.ToString("N0") + " VNĐ" });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartToSession(cart);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // Cần là async Task<IActionResult> nếu bạn dùng await _userManager.GetUserAsync(User)
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            // Đảm bảo User và User.Identity không null một cách an toàn
            // Mặc dù User và User.Identity ít khi null trong request web, thêm kiểm tra này để làm hài lòng trình biên dịch.
            if (User?.Identity == null || !User.Identity.IsAuthenticated) // Thêm toán tử ?. để kiểm tra null an toàn
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            // Dòng 120 của bạn nằm ở đây, sau khi chúng ta đã đảm bảo người dùng được xác thực.
            var currentUser = await _userManager.GetUserAsync(User);

            // Kể cả khi người dùng được xác thực, currentUser có thể null nếu tài khoản không tồn tại trong DB.
            if (currentUser == null)
            {
                // Điều hướng người dùng đến trang đăng nhập/lỗi hoặc đăng xuất họ ra.
                // Tùy chọn: Bạn có thể đăng xuất người dùng để họ đăng nhập lại nếu tài khoản không hợp lệ.
                // await _signInManager.SignOutAsync(); // Nếu bạn có inject SignInManager
                TempData["ErrorMessage"] = "Tài khoản của bạn không tồn tại hoặc đã bị xóa. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            var checkoutVm = new CheckoutPageViewModel
            {
                CartItems = cart,
                // Sau khi kiểm tra currentUser != null, chúng ta có thể an toàn bỏ ! khi truy cập thuộc tính
                // Tuy nhiên, các thuộc tính như FullName, Email, PhoneNumber, ShippingAddress vẫn có thể là null
                // nếu bạn khai báo chúng là nullable (string?).
                // Sử dụng ?? string.Empty hoặc kiểm tra null nếu chúng là non-nullable
                CustomerName = currentUser.FullName ?? string.Empty,
                Email = currentUser.Email, // Nếu Email trong ApplicationUser là string?, thì ổn
                PhoneNumber = currentUser.PhoneNumber ?? string.Empty,
                ShippingAddress = currentUser.ShippingAddress ?? string.Empty
            };

            return View(checkoutVm);
        }
    }
} // <<< THÊM DẤU ĐÓNG NGOẶC NÀY VÀO CUỐI CÙNG NẾU CHƯA CÓ >>>