namespace ElectronicsStore.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public required string Name { get; set; } // <<< Thêm 'required' >>>

        [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự")]
        public string? Description { get; set; } // <<< Đánh dấu là nullable (có thể null) >>>

        [StringLength(250, ErrorMessage = "Slug không được vượt quá 250 ký ký tự")]
        public required string Slug { get; set; } // <<< Thêm 'required' >>>

        [StringLength(500, ErrorMessage = "URL ảnh danh mục không được vượt quá 500 ký tự")]
        public string? ImageUrl { get; set; } // <<< Đánh dấu là nullable (có thể null) >>>

        public ICollection<Product> Products { get; set; } = new List<Product>(); // <<< Khởi tạo tập hợp rỗng >>>
    }

}