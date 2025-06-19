using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace OrderingSystemMvc.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "請輸入分類名稱")]
        public string Name { get; set; }  // 分類名稱
        [Range(0, 100, ErrorMessage = "排序請輸入 0~100 之間")]
        public int SortOrder { get; set; }  // 排序編號
        [ValidateNever]
        public ICollection<MenuItem> MenuItems { get; set; }
    }

}