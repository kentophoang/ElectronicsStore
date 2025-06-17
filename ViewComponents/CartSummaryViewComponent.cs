using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using ElectronicsStore.Models; // Adjust according to your project structure

// Đặt View Component vào namespace nếu các Controller/Models của bạn cũng dùng namespace
namespace ElectronicsStore.ViewComponents
{
public class CartSummaryViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        // THAY ĐỔI TẠI ĐÂY: Thêm toán tử null-forgiving '!' sau DeserializeObject
        var cart = cartJson == null ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson)!;

        // Dòng này bây giờ sẽ an toàn hơn vì 'cart' được đảm bảo không null
        int cartItemCount = cart.Sum(item => item.Quantity);
        return View(cartItemCount);
    }
}
} // Dấu đóng ngoặc namespace