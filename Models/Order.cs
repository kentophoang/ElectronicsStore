using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using ElectronicsStore.Models; // Đảm bảo đã có


namespace ElectronicsStore.Models
{
    public enum OrderStatus
    {
        Pending,        // Chờ xác nhận
        Confirmed,      // Đã xác nhận
        Processing,     // Đang xử lý
        Shipped,        // Đã giao hàng
        Delivered,      // Đã giao thành công
        Cancelled,      // Đã hủy
        Returned        // Đã trả hàng
    }

    public class Order
    {
        public int Id { get; set; }
        public required string UserId { get; set; } // <<< Thêm 'required' >>>
        public ApplicationUser? User { get; set; } // <<< Đánh dấu là nullable '?' >>>

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(250)]
        public required string ShippingAddress { get; set; } // <<< Thêm 'required' >>>

        [Required]
        [StringLength(100)]
        public required string CustomerName { get; set; } // <<< Thêm 'required' >>>

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; } // <<< Thêm 'required' >>>

        [EmailAddress]
        public string? Email { get; set; } // Giữ nguyên nullable nếu đã thay đổi trước đó

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public required string PaymentMethod { get; set; } // <<< Thêm 'required' >>>
        public bool IsPaid { get; set; } = false;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // <<< Khởi tạo tập hợp rỗng >>>
    }

}