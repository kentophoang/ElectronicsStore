using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public required string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(250)]
        public required string ShippingAddress { get; set; }

        [Required]
        [StringLength(100)]
        public required string CustomerName { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public required string PaymentMethod { get; set; }
        public bool IsPaid { get; set; } = false;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}