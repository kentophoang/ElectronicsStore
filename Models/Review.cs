// Models/Review.cs
namespace ElectronicsStore.Models // Make sure this matches your model namespace
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity; // Needed for ApplicationUser

    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Foreign key to Product
        public required Product Product { get; set; } // <<< THÊM 'required' >>>

        public required string UserId { get; set; } // <<< THÊM 'required' >>>
        public ApplicationUser? User { get; set; } // <<< ĐÁNH DẤU LÀ NULLABLE '?' >>>

        [Range(1, 5, ErrorMessage = "Đánh giá sao phải từ 1 đến 5")] // <<< ĐÃ SỬA LỖI TẠI ĐÂY >>>
        public int Rating { get; set; } // Số sao (1-5)

        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public required string Comment { get; set; } // <<< THÊM 'required' >>>

        public DateTime ReviewDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false; // Admin có thể duyệt đánh giá
    }
}