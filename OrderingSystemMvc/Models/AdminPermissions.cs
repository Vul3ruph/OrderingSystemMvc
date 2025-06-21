using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.Models
{
    // 管理員權限設定（保持獨立表）
    public class AdminPermissions
    {
        [Key]
        public int Id { get; set; }

        // 外鍵：關聯到 ApplicationUser
        [Required]
        public string UserId { get; set; }

        // 訂單管理權限
        public bool CanViewOrders { get; set; } = true;
        public bool CanEditOrders { get; set; } = false;
        public bool CanDeleteOrders { get; set; } = false;

        // 菜單項目權限
        public bool CanViewMenuItems { get; set; } = true;
        public bool CanEditMenuItems { get; set; } = false;
        public bool CanDeleteMenuItems { get; set; } = false;

        // 分類管理權限
        public bool CanViewCategories { get; set; } = true;
        public bool CanEditCategories { get; set; } = false;
        public bool CanDeleteCategories { get; set; } = false;

        // 報表權限
        public bool CanViewReports { get; set; } = true;
        public bool CanExportData { get; set; } = false;

        // 系統管理權限
        public bool CanManageAdmins { get; set; } = false;
        public bool CanManageSettings { get; set; } = false;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
