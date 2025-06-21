// Areas/User/Controllers/AccountController.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.Services;
using System.Security.Claims;

namespace OrderingSystemMvc.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAdminService _adminService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAdminService adminService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            try
            {
                Console.WriteLine($"🔍 嘗試登入: {email}");

                // 1. 尋找用戶
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(email);
                }

                if (user == null)
                {
                    Console.WriteLine("❌ 找不到用戶");
                    ModelState.AddModelError(string.Empty, "Email 或密碼錯誤");
                    return View();
                }

                Console.WriteLine($"找到用戶: {user.UserName}, UserType: {user.UserType}");

                // 2. 驗證密碼
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    Console.WriteLine("❌ 密碼錯誤");
                    ModelState.AddModelError(string.Empty, "Email 或密碼錯誤");
                    return View();
                }

                // 3. 檢查帳號狀態
                if (!user.IsActive)
                {
                    Console.WriteLine("❌ 用戶未啟用");
                    ModelState.AddModelError(string.Empty, "帳號已停用，請聯繫客服");
                    return View();
                }

                // 4. 更新最後登入時間
                user.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(user);

                // 5. 根據用戶類型決定登入方式和導向
                // 在 AccountController.cs 的 Login 方法中，修改管理員登入後的導向：

                // 6. 根據用戶類型決定登入方式和導向
                if (user.UserType == "Admin" || user.UserType == "SuperAdmin")
                {
                        Console.WriteLine($"🔑 管理員登入流程: {user.UserType}");

                        // 管理員使用 AdminCookies 認證
                        var adminClaims = new List<Claim>
                  {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("DisplayName", user.DisplayName ?? user.UserName ?? ""),
                    new Claim("UserType", user.UserType)
                };

                    var adminIdentity = new ClaimsIdentity(adminClaims, "AdminCookies");
                    var adminAuthProperties = new AuthenticationProperties
                    {
                        IsPersistent = rememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync("AdminCookies", new ClaimsPrincipal(adminIdentity), adminAuthProperties);
                    Console.WriteLine("✅ AdminCookies 認證完成");

                    // 🎯 導向 Dashboard 首頁
                    Console.WriteLine("🎯 導向管理後台首頁: /Admin/Dashboard");
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    Console.WriteLine($"✅ 一般會員登入: {user.UserName}");

                    // 一般會員使用預設 Identity 認證
                    await _signInManager.SignInAsync(user, rememberMe);

                    // 導向前台菜單
                    return RedirectToAction("Index", "Menu", new { area = "User" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 登入過程發生錯誤: {ex.Message}");
                ModelState.AddModelError(string.Empty, "登入過程發生錯誤，請稍後再試");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // 檢查當前用戶類型，決定登出方式
                var userType = User.FindFirst("UserType")?.Value;

                if (userType == "Admin" || userType == "SuperAdmin")
                {
                    // 管理員登出
                    await HttpContext.SignOutAsync("AdminCookies");
                    Console.WriteLine("✅ 管理員已登出");
                }
                else
                {
                    // 一般會員登出
                    await _signInManager.SignOutAsync();
                    Console.WriteLine("✅ 會員已登出");
                }

                // 清除所有認證（保險起見）
                await HttpContext.SignOutAsync("AdminCookies");
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登出時發生錯誤: {ex.Message}");
            }

            return RedirectToAction("Index", "Menu", new { area = "User" });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string displayName)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "密碼確認不符");
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                UserType = "Customer", // 註冊的都是一般顧客
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                Console.WriteLine($"✅ 新用戶註冊成功: {email}");

                // 自動登入
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Menu", new { area = "User" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}