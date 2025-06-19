using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入餐點名稱")]
        [Display(Name = "餐點名稱")]
        public string Name { get; set; }
        [Display(Name = "餐點描述")]
        public string? Description { get; set; }


        [Required(ErrorMessage = "請輸入價格")]
        [Range(0, 9999, ErrorMessage = "價格請輸入 0~9999")]
        [Display(Name = "價格")]
        public decimal Price { get; set; }
        [Display(Name = "圖片上傳")]

        public string? ImageUrl { get; set; }

        [Display(Name = "分類")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }
        [Display(Name = "排序")]
        public int SortOrder { get; set; }
        public bool IsAvailable { get; set; } = true; // 是否可供點餐

        public ICollection<MenuItemOption>? MenuItemOptions { get; set; }
    }
}