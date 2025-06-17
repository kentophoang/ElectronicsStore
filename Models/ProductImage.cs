using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Models // <<< THÊM DÒNG NÀY VÀO ĐÂY >>>
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } // Dòng gây lỗi nếu Product không được tìm thấy

        [Required(ErrorMessage = "URL ảnh là bắt buộc")]
        [StringLength(500, ErrorMessage = "URL ảnh không được vượt quá 500 ký tự")]
        public string ImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự sắp xếp không hợp lệ")]
        public int SortOrder { get; set; }
    }
} // <<< THÊM DẤU ĐÓNG NGOẶC NÀY VÀO CUỐI CÙNG >>>