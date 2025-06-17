using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models // ĐẢM BẢO DÒNG NÀY CÓ VÀ CHÍNH XÁC
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; } // Khóa ngoại tới Order
        public Order Order { get; set; }

        public int ProductId { get; set; } // Khóa ngoại tới Product
        public Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; } // Giá tại thời điểm đặt hàng

        public decimal Total => Quantity * Price;
    }
} // ĐẢM BẢO DẤU ĐÓNG NGOẶC CÓ Ở CUỐI