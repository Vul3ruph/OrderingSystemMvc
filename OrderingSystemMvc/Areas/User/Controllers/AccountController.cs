// Areas/User/Controllers/AccountController.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.Services;
using OrderingSystemMvc.ViewModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace OrderingSystemMvc.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAdminService _adminService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAdminService adminService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _adminService = adminService;
            _context = context;
        }
        // 登入頁面
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        // 登入處理
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            try
            {
                Console.WriteLine($"🔍 嘗試登入: {email}");

                // 尋找用戶
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

                // 驗證密碼
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    Console.WriteLine("❌ 密碼錯誤");
                    ModelState.AddModelError(string.Empty, "Email 或密碼錯誤");
                    return View();
                }

                // 檢查帳號狀態
                if (!user.IsActive)
                {
                    Console.WriteLine("❌ 用戶未啟用");
                    ModelState.AddModelError(string.Empty, "帳號已停用，請聯繫客服");
                    return View();
                }

                // 更新最後登入時間
                user.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(user);

                // 根據用戶類型登入
                if (user.UserType == "Admin" || user.UserType == "SuperAdmin")
                {
                    // 管理員登入
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
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else
                {
                    // 一般會員登入
                    await _signInManager.SignInAsync(user, rememberMe);
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
        // 登出處理
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userType = User.FindFirst("UserType")?.Value;

                if (userType == "Admin" || userType == "SuperAdmin")
                {
                    await HttpContext.SignOutAsync("AdminCookies");
                }
                else
                {
                    await _signInManager.SignOutAsync();
                }

                // 清除所有認證
                await HttpContext.SignOutAsync("AdminCookies");
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登出時發生錯誤: {ex.Message}");
            }

            return RedirectToAction("Index", "Menu", new { area = "User" });
        }

        // 註冊頁面
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 註冊處理
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string phoneNumber)
        {
            if (ModelState.IsValid)
            {
                // 檢查重複
                var existingEmailUser = await _userManager.FindByEmailAsync(email);
                if (existingEmailUser != null)
                {
                    ModelState.AddModelError("Email", "此電子郵件已被使用");
                    return View();
                }

                var existingPhoneUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
                if (existingPhoneUser != null)
                {
                    ModelState.AddModelError("PhoneNumber", "此手機號碼已被使用");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "密碼確認不符");
                    return View();
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    UserType = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    Console.WriteLine($"✅ 新用戶註冊成功: {email}");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Menu", new { area = "User" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }


        // 修改密碼頁面
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        // 修改密碼處理
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // 驗證目前密碼是否正確
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "目前密碼不正確");
                    return View(model);
                }

                // 檢查新密碼是否與舊密碼相同
                if (model.CurrentPassword == model.NewPassword)
                {
                    ModelState.AddModelError("NewPassword", "新密碼不能與目前密碼相同");
                    return View(model);
                }

                // 更改密碼
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // 更新安全戳記，讓其他地方的登入失效
                    await _userManager.UpdateSecurityStampAsync(user);

                    // 重新登入以刷新安全 token
                    await _signInManager.RefreshSignInAsync(user);

                    TempData["Success"] = "密碼修改成功！";

                    // 可以選擇重新導向到 Profile 頁面或留在同一頁
                    return RedirectToAction("Profile");
                }

                // 處理密碼修改失敗的錯誤
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, TranslatePasswordError(error.Description));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"修改密碼時發生錯誤: {ex.Message}");
                ModelState.AddModelError(string.Empty, "修改密碼過程中發生錯誤，請稍後再試");
            }

            return View(model);
        }

        // 翻譯密碼錯誤訊息
        private string TranslatePasswordError(string error)
        {
            return error switch
            {
                var e when e.Contains("PasswordTooShort") => "密碼長度太短",
                var e when e.Contains("PasswordRequiresDigit") => "密碼必須包含數字",
                var e when e.Contains("PasswordRequiresLower") => "密碼必須包含小寫字母",
                var e when e.Contains("PasswordRequiresUpper") => "密碼必須包含大寫字母",
                var e when e.Contains("PasswordRequiresNonAlphanumeric") => "密碼必須包含特殊字元",
                _ => error
            };
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // 查詢用戶的訂單統計資料
            var userOrders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .ToListAsync();

            // 計算統計資料
            var totalOrders = userOrders.Count;
            var totalSpent = userOrders.Sum(o => o.TotalAmount);
            var memberLevel = CalculateMemberLevel(totalSpent, totalOrders);

            var viewModel = new ProfileViewModel
            {
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                DisplayName = user.DisplayName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,

                // 動態統計資料
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                MemberLevel = memberLevel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 如果模型驗證失敗，重新填充統計資料
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var userOrders = await _context.Orders
                        .Where(o => o.UserId == user.Id)
                        .ToListAsync();

                    model.TotalOrders = userOrders.Count;
                    model.TotalSpent = userOrders.Sum(o => o.TotalAmount);
                    model.MemberLevel = CalculateMemberLevel(model.TotalSpent, model.TotalOrders);
                    model.CreatedAt = user.CreatedAt;
                    model.LastLoginAt = user.LastLoginAt;
                }

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                // 檢查手機號碼是否被其他用戶使用
                if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber != currentUser.PhoneNumber)
                {
                    var existingUser = await _userManager.Users
                        .Where(u => u.Id != currentUser.Id && u.PhoneNumber == model.PhoneNumber)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("PhoneNumber", "此手機號碼已被其他用戶使用");

                        // 重新填充統計資料
                        var userOrders = await _context.Orders
                            .Where(o => o.UserId == currentUser.Id)
                            .ToListAsync();

                        model.TotalOrders = userOrders.Count;
                        model.TotalSpent = userOrders.Sum(o => o.TotalAmount);
                        model.MemberLevel = CalculateMemberLevel(model.TotalSpent, model.TotalOrders);
                        model.CreatedAt = currentUser.CreatedAt;
                        model.LastLoginAt = currentUser.LastLoginAt;

                        return View(model);
                    }
                }

                // 更新用戶資料
                currentUser.PhoneNumber = model.PhoneNumber;
                currentUser.DisplayName = model.DisplayName;

                var result = await _userManager.UpdateAsync(currentUser);

                if (result.Succeeded)
                {
                    TempData["Success"] = "個人資料更新成功！";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新個人資料時發生錯誤: {ex.Message}");
                ModelState.AddModelError(string.Empty, "更新過程中發生錯誤，請稍後再試");
            }

            // 如果有錯誤，重新填充統計資料
            var orders = await _context.Orders
                .Where(o => o.UserId == currentUser.Id)
                .ToListAsync();

            model.TotalOrders = orders.Count;
            model.TotalSpent = orders.Sum(o => o.TotalAmount);
            model.MemberLevel = CalculateMemberLevel(model.TotalSpent, model.TotalOrders);
            model.CreatedAt = currentUser.CreatedAt;
            model.LastLoginAt = currentUser.LastLoginAt;

            return View(model);
        }

        // Email 檢查
        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Json(new { exists = user != null });
        }

        // 電話號碼檢查
        [HttpPost]
        public async Task<IActionResult> CheckPhoneNumberExists([FromBody] string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return Json(new { exists = false, message = "電話號碼不能為空" });
                }

                // 清理格式
                var cleanPhoneNumber = phoneNumber.Trim()
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "");

                // 檢查格式
                var phoneRegex = new Regex(@"^09\d{8}$");
                if (!phoneRegex.IsMatch(cleanPhoneNumber))
                {
                    return Json(new { exists = false, message = "電話號碼格式不正確" });
                }

                // 檢查是否存在
                var existingUser = await _userManager.Users
                    .Where(u => u.PhoneNumber == cleanPhoneNumber)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return Json(new { exists = true, message = "此電話號碼已被註冊" });
                }

                return Json(new { exists = false, message = "電話號碼可以使用" });
            }
            catch (Exception ex)
            {
                return Json(new { exists = false, message = "檢查過程中發生錯誤" });
            }
        }



        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // 計算會員等級的私有方法
        private string CalculateMemberLevel(decimal totalSpent, int totalOrders)
        {
            if (totalSpent >= 10000 || totalOrders >= 50)
            {
                return "鑽石會員";
            }
            else if (totalSpent >= 5000 || totalOrders >= 25)
            {
                return "金牌會員";
            }
            else if (totalSpent >= 2000 || totalOrders >= 10)
            {
                return "銀牌會員";
            }
            else if (totalSpent >= 500 || totalOrders >= 3)
            {
                return "銅牌會員";
            }
            else
            {
                return "新手會員";
            }
        }
    }
}