using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingSystemMvc.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<OrderOptionItem> OrderOptionItems { get; set; } = new();



        [NotMapped]
        public string OptionSummary
        {
            get
            {
                if (OrderOptionItems == null || !OrderOptionItems.Any())
                    return string.Empty;

                return string.Join(", ", OrderOptionItems.Select(o => o.OptionItem.Name));
            }
        }

    }
}