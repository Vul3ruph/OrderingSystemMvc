using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.ViewModels
{
    // 收據的 ViewModel
    public class ReceiptViewModel
    {
        
        public Order Order { get; set; }
        public string ReceiptNumber { get; set; } = "";
        public DateTime PrintTime { get; set; }
        public string StoreName { get; set; } = "";
        public string StoreAddress { get; set; } = "";
        public string StorePhone { get; set; } = "";
        public string StoreHours { get; set; } = "";
        public string CustomerName { get; set; } = "";
    }
}
