namespace OrderingSystemMvc.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsSingleChoice { get; set; } = true;
        public int SortOrder { get; set; }

        public ICollection<OptionItem>? OptionItems { get; set; }
    }

}