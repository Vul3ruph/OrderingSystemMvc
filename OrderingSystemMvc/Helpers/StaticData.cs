using OrderingSystemMvc.Models;
using System.Collections.Generic;

namespace OrderingSystemMvc.Helpers
{
    public static class StaticData
    {
        public static Dictionary<int, OptionItem> OptionItemDict { get; set; } = new();
    }
}
