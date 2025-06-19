using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên Danh mục")]
        public required string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Slug { get; set; }

        [StringLength(500)]
        [Display(Name = "URL Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category? Parent { get; set; }

        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}