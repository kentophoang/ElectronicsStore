namespace ElectronicsStore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200)]
        public required string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(500)]
        public required string ShortDescription { get; set; }

        [DataType(DataType.Html)]
        public string? FullDescription { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal Price { get; set; }

        // Giữ lại vì bạn đã quyết định mọi sản phẩm đều phải có giá gốc
        [Required(ErrorMessage = "Giá gốc là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn 0")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [StringLength(500)]
        public required string MainImageUrl { get; set; }
        public string? Description { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [StringLength(250)]
        public required string Slug { get; set; }

        // Ngày giờ sẽ được quản lý bởi DbContext
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
    }
}