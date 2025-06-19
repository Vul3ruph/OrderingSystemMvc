using Microsoft.AspNetCore.Identity;

namespace OrderingSystemMvc.Models
{
    /// <summary>
    /// 繼承自 IdentityUser 的自訂使用者類別，
    /// 以後如果想多儲存欄位（暱稱、手機…）可以在這裡加。
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}