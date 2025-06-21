using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.ViewModels
{
    // 註冊 ViewModel
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, ErrorMessage = "密碼長度至少 {2} 個字元", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請確認密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不符")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "請輸入有效的手機號碼")]
        [Display(Name = "手機號碼")]
        public string? PhoneNumber { get; set; }
    }

    // 登入 ViewModel
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }

    // 會員資料 ViewModel
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "請輸入有效的手機號碼")]
        [Display(Name = "手機號碼")]
        public string? PhoneNumber { get; set; }
    }
}