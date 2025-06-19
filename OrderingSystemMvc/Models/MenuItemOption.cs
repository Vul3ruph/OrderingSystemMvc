namespace OrderingSystemMvc.Models
{
    public class MenuItemOption
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public int OptionId { get; set; }

        public MenuItem? MenuItem { get; set; }
        public Option? Option { get; set; }
    }

}