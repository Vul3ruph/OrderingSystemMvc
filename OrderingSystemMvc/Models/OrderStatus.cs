using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty; // 內部識別用

        [Required]
        public string Name { get; set; } = string.Empty; // 顯示用名稱（中文）

        public string ColorClass { get; set; } = "bg-secondary"; // 顏色類別（用於 Bootstrap 標籤）

        // 可選：關聯到訂單
        public ICollection<Order>? Orders { get; set; }
    }
}