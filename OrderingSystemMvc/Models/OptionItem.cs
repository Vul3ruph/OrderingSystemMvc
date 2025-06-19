namespace OrderingSystemMvc.Models
{
    public class OptionItem
    {
        public int Id { get; set; }
        public int OptionId { get; set; }
        public string Name { get; set; } = "";
        public decimal ExtraPrice { get; set; } = 0;

        public Option? Option { get; set; }
    }

}