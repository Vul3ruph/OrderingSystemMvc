using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.Models
{
    /// <summary>
    /// 繼承自 IdentityUser 的自訂使用者類別，
    /// 以後如果想多儲存欄位（暱稱、手機…）可以在這裡加。
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // 顯示名稱
        [StringLength(100)]
        public string? DisplayName { get; set; }

        // 用戶類型：Customer, Admin, SuperAdmin
        [StringLength(20)]
        public string UserType { get; set; } = "Customer";

        // 是否啟用
        public bool IsActive { get; set; } = true;

        // 建立時間
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 最後登入時間
        public DateTime? LastLoginAt { get; set; }

        // 導航屬性：訂單
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        // 導航屬性：管理員權限
        public virtual AdminPermissions? AdminPermissions { get; set; }
    }


}