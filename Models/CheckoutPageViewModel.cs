namespace ElectronicsStore.Models // <<< ĐẢM BẢO NAMESPACE NÀY CHÍNH XÁC >>>
{
    using System.Collections.Generic; // Cần thiết cho List<CartItem>

    public class CheckoutPageViewModel : CheckoutViewModel // Kế thừa từ CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>(); // <<< Khởi tạo danh sách rỗng >>>
    }
}