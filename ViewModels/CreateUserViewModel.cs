using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}