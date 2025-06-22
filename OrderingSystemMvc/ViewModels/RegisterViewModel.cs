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

        // 新增這些屬性來支持你的 View
        [Display(Name = "顯示名稱")]
        public string? DisplayName { get; set; }

        [Display(Name = "註冊時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "最後登入時間")]
        public DateTime? LastLoginAt { get; set; }

        // 統計資料 - 用於顯示在卡片中
        public int TotalOrders { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public string MemberLevel { get; set; } = "新手";
    }

    // 忘記密碼 ViewModel
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "請輸入 Email")]
        [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
    // 重設密碼 ViewModel
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "請輸入目前密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "目前密碼")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入新密碼")]
        [StringLength(100, ErrorMessage = "密碼長度至少需要 {2} 個字元", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不符")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}