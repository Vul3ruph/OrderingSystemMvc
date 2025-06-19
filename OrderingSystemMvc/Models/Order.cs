using OrderingSystemMvc.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderingSystemMvc.Models
{
    public class Order
    {
        public int Id { get; set; }
        // ─── 使用者 ──────────────────────────────
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        // ─── 時間、狀態 ──────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int OrderStatusId { get; set; }

        [ForeignKey("OrderStatusId")]
        public OrderStatus Status { get; set; }
        // ─── 金額 ────────────────────────────────
        public decimal TotalAmount { get; set; }

        // ─── 明細 ────────────────────────────────
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}