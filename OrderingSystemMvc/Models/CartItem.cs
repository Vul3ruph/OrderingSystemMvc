using OrderingSystemMvc.Helpers;

namespace OrderingSystemMvc.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? ImageUrl { get; set; }
        public decimal ExtraTotal
        {
            get
            {
                return SelectedOptionItemIds
                    .Where(id => StaticData.OptionItemDict.ContainsKey(id))
                    .Sum(id => StaticData.OptionItemDict[id].ExtraPrice);
            }
        }

        public List<int> SelectedOptionItemIds { get; set; } = new(); // ✅ 選項項目 ID 記錄
        public string? OptionSummary { get; set; }

    }
}