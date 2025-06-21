
namespace OrderingSystemMvc.Models
{
    public class OrderOptionItem
    {
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }

        public int OptionItemId { get; set; }
        public OptionItem OptionItem { get; set; }

        public decimal ExtraPrice { get; set; }
    }
}