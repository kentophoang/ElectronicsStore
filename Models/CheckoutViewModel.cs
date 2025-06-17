// Models/CheckoutViewModel.cs
namespace ElectronicsStore.Models // <<< ĐẢM BẢO NAMESPACE NÀY CHÍNH XÁC >>>
{
    using System.ComponentModel.DataAnnotations;

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        public string CustomerName { get; set; } = string.Empty; // Gán mặc định hoặc Required

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; } = string.Empty; // Gán mặc định hoặc Required

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc.")]
        [StringLength(250, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 250 ký tự.")]
        public string ShippingAddress { get; set; } = string.Empty; // Gán mặc định hoặc Required

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string? Email { get; set; } // Có thể null

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
        public string PaymentMethod { get; set; } = string.Empty; // Gán mặc định hoặc Required
    }
}