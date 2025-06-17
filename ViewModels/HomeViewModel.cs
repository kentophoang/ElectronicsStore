// Đây là nơi bạn định nghĩa các Model của ứng dụng, như Product, Category, v.v.
using ElectronicsStore.Models;
using System.Collections.Generic;

// Namespace nên khớp với cấu trúc thư mục của project
namespace ElectronicsStore.ViewModels
{
   
    public class HomeViewModel
    {
        /// <summary>
        /// Danh sách các sản phẩm nổi bật để hiển thị ở khu vực chính của trang chủ.
        /// </summary>
        public List<Product> FeaturedProducts { get; set; }

        /// <summary>
        /// Danh sách các sản phẩm mới nhất, có thể dùng cho khu vực "Hàng mới về".
        /// </summary>
        public List<Product> NewArrivals { get; set; }

        /// <summary>
        /// Danh sách các sản phẩm đang được giảm giá mạnh, dùng cho khu vực "Khuyến mãi hot".
        /// </summary>
        public List<Product> TopDeals { get; set; }

        /// <summary>
        /// Danh sách các danh mục sản phẩm chính để hiển thị trên menu hoặc các khu vực điều hướng.
        /// </summary>
        public List<Category> Categories { get; set; }

        // Bạn có thể thêm các thuộc tính khác nếu trang chủ cần hiển thị thêm thông tin.
        // Ví dụ:
        // public List<Banner> PromotionalBanners { get; set; }

        /// <summary>
        /// Hàm khởi tạo để đảm bảo các danh sách không bao giờ bị null.
        /// Điều này giúp tránh lỗi NullReferenceException trong View khi truy cập.
        /// </summary>
        public HomeViewModel()
        {
            FeaturedProducts = new List<Product>();
            NewArrivals = new List<Product>();
            TopDeals = new List<Product>();
            Categories = new List<Category>();
        }
    }
}