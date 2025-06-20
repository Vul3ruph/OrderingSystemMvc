using OrderingSystemMvc.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace OrderingSystemMvc.Data;

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        /* ---------- 建立分類（若不存在就建立） ---------- */
        var categoryDict = new Dictionary<string, Category>();

        void EnsureCategory(string name, int sortOrder)
        {
            var cat = context.Categories.FirstOrDefault(c => c.Name == name);
            if (cat == null)
            {
                cat = new Category { Name = name, SortOrder = sortOrder };
                context.Categories.Add(cat);
                context.SaveChanges();
            }
            categoryDict[name] = cat;
        }

        EnsureCategory("早餐", 1);   // id 會由 DB 產生
        EnsureCategory("輕食", 2);

        /* ---------- 建立餐點（若不存在就建立） ---------- */
        var sampleMenuItems = new[]
        {
            new { Name="經典美式早餐", Desc="雙蛋、酥脆培根、香腸、烤番茄、酪梨、烤吐司", Price=280m, Img="/images/menu/9ebec16d.jpg", Cat="早餐"},
            new { Name="酪梨班尼迪克蛋", Desc="英式鬆餅、水波蛋、酪梨、荷蘭醬、煙燻鮭魚",   Price=320m, Img="/images/menu/2c72df15.jpg", Cat="早餐"},
            new { Name="法式吐司套餐", Desc="厚切法式吐司、季節水果、楓糖漿、鮮奶油",       Price=260m, Img="/images/menu/9413afc7.jpg", Cat="早餐"},
            new { Name="蔬食沙拉碗",   Desc="黎麥、酪梨、烤南瓜、櫛瓜蕃茄、嫩菠菜、藍莓醬",   Price=240m, Img="/images/menu/460fc839.jpg", Cat="輕食"},
            new { Name="手工捲餅米漢", Desc="義大利馬芝卡起司、手拍豬肉餅、濃縮咖啡、可可粉", Price=180m, Img="/images/menu/23dc2d7b.jpg", Cat="輕食"},
            new { Name="季節水果鬆餅", Desc="比利時鬆餅、季節水果、鮮奶油、楓糖漿",           Price=220m, Img="/images/menu/0eb6a6af.jpg", Cat="輕食"},
            new { Name="手沖單品咖啡", Desc="每日精選咖啡豆，由專業咖啡師手沖",               Price=150m, Img="/images/menu/b38e7351.jpg", Cat="輕食"},
            new { Name="鮮榨果汁",     Desc="每日新鮮水果，現榨果汁",                         Price=130m, Img="/images/menu/45fed28a.jpg", Cat="輕食"},
        };

        foreach (var m in sampleMenuItems)
        {
            if (!context.MenuItems.Any(mi => mi.Name == m.Name))
            {
                context.MenuItems.Add(new MenuItem
                {
                    Name = m.Name,
                    Description = m.Desc,
                    Price = m.Price,
                    ImageUrl = m.Img,
                    CategoryId = categoryDict[m.Cat].Id,
                    SortOrder = 0,
                    IsAvailable = true
                });
            }
        }
        context.SaveChanges();

        /* ---------- 建立選項（若不存在就建立） ---------- */
        var eggOption = context.Options.FirstOrDefault(o => o.Name == "選擇蛋的烹調方式");
        if (eggOption == null)
        {
            eggOption = new Option
            {
                Name = "選擇蛋的烹調方式",
                IsSingleChoice = true,
                SortOrder = 1
            };
            context.Options.Add(eggOption);
            context.SaveChanges();

            context.OptionItems.AddRange(
                new OptionItem { Name = "太陽蛋", ExtraPrice = 0, OptionId = eggOption.Id },
                new OptionItem { Name = "炒蛋", ExtraPrice = 0, OptionId = eggOption.Id },
                new OptionItem { Name = "水波蛋", ExtraPrice = 0, OptionId = eggOption.Id }
            );
            context.SaveChanges();
        }

        /* ---------- 建立餐點與選項關聯（只給第一道餐點示範） ---------- */
        var firstBreakfast = context.MenuItems.First(mi => mi.Name == "經典美式早餐");
        bool linked = context.MenuItemOptions.Any(mio =>
                       mio.MenuItemId == firstBreakfast.Id && mio.OptionId == eggOption.Id);

        if (!linked)
        {
            context.MenuItemOptions.Add(new MenuItemOption
            {
                MenuItemId = firstBreakfast.Id,
                OptionId = eggOption.Id
            });
            context.SaveChanges();
        }
    }
}
