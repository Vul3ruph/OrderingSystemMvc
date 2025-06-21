using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.ViewModels;

namespace OrderingSystemMvc.Areas.User.Controllers
{
    [Area("User")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: 註冊頁面
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Menu");
            }
            return View();
        }

        // POST: 處理註冊
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 檢查 Email 是否已被使用
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "此 Email 已被註冊");
                return View(model);
            }

            // 建立新用戶
            var user = new ApplicationUser
            {
                UserName = model.Email, // 使用 Email 作為用戶名
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true // 暫時設為已確認，實際專案可能需要 Email 驗證
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 註冊成功，自動登入
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = "🎉 註冊成功！歡迎加入我們！";
                return RedirectToAction("Index", "Menu");
            }

            // 註冊失敗，顯示錯誤
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: 登入頁面
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Menu");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: 處理登入
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                TempData["Success"] = "登入成功！";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Menu");
            }

            ModelState.AddModelError(string.Empty, "Email 或密碼錯誤");
            return View(model);
        }

        // POST: 登出
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "已成功登出";
            return RedirectToAction("Index", "Menu");
        }

        // GET: 會員資料頁面
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // POST: 更新會員資料
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "資料更新成功！";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}