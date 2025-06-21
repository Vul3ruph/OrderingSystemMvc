using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Helpers
{
    public static class CartHelper
    {
        private const string CartKey = "MyCart";

        public static List<CartItem> GetCart(HttpContext context)
        {
            var sessionData = context.Session.GetString(CartKey);
            return sessionData != null
                ? JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!
                : new List<CartItem>();
        }

        public static void SaveCart(HttpContext context, List<CartItem> cart)
        {
            var data = JsonConvert.SerializeObject(cart);
            context.Session.SetString(CartKey, data);
        }

        public static void AddToCart(HttpContext context, CartItem item)
        {
            var cart = GetCart(context);

            // 比對：商品 Id 一樣且選項內容也一樣才合併
            var existing = cart.FirstOrDefault(c =>
                c.MenuItemId == item.MenuItemId &&
                c.SelectedOptionItemIds.Count == item.SelectedOptionItemIds.Count &&
                !c.SelectedOptionItemIds.Except(item.SelectedOptionItemIds).Any()
            );


            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                cart.Add(item);
            }

            SaveCart(context, cart);

           
        }

        public static int GetCartCount(HttpContext context)
        {
            var cart = GetCart(context);
            return cart.Sum(c => c.Quantity);
        }

    }
}
