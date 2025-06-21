using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Helpers;
using OrderingSystemMvc.Models;
using OrderingSystemMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// 注入服務
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity 設定（前台用戶）
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 密碼設定
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // 用戶設定
    options.User.RequireUniqueEmail = true;

    // 登入設定
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 設定前台登入路徑 (預設的 Identity Cookie)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Account/Login";
    options.LogoutPath = "/User/Account/Logout";
    options.AccessDeniedPath = "/User/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// 管理員專用的 Cookie 認證方案（後台）
builder.Services.AddAuthentication()
    .AddCookie("AdminCookies", options =>
    {
        // ✅ 修改：管理員也導向統一登入頁面
        options.LoginPath = "/User/Account/Login";  // 改成統一登入頁面
        options.LogoutPath = "/User/Account/Logout"; // 改成統一登出
        options.AccessDeniedPath = "/User/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AdminAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Session 設定
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 註冊管理員服務
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 認證授權
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// 路由設定
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Menu}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { area = "User", controller = "Menu", action = "Index" });

app.MapControllers();

// 建立資料庫與種子資料
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // ✅ 加回來
        var adminService = services.GetRequiredService<IAdminService>();

        // 確保資料庫存在
        await db.Database.EnsureCreatedAsync();

        // 初始化 StaticData
        StaticData.OptionItemDict = db.OptionItems
            .Include(o => o.Option)
            .ToDictionary(o => o.Id);

        // 初始資料庫
        DataSeeder.Seed(db);

        // ✅ 建立預設管理員帳號 + 檢查現有用戶
        await CreateDefaultAdminAsync(userManager, adminService);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "初始化資料時發生錯誤");
    }
}

app.Run();

// ✅ 修正：加強版建立管理員方法
static async Task CreateDefaultAdminAsync(UserManager<ApplicationUser> userManager, IAdminService adminService)
{
    try
    {
        // 檢查並修正現有用戶
        Console.WriteLine("=== 檢查現有用戶資料 ===");
        var allUsers = await userManager.Users.ToListAsync();

        foreach (var user in allUsers)
        {
            Console.WriteLine($"用戶: {user.UserName} | Email: {user.Email} | UserType: {user.UserType ?? "NULL"}");

            // 修正現有用戶的 UserType
            if (string.IsNullOrEmpty(user.UserType))
            {
                user.UserType = "Customer"; // 預設為顧客
                await userManager.UpdateAsync(user);
                Console.WriteLine($"✅ 已修正 {user.UserName} 的 UserType 為 Customer");
            }
        }

        // 檢查是否已存在管理員
        var existingAdmin = await adminService.GetAdminByUsernameAsync("admin");
        if (existingAdmin != null)
        {
            Console.WriteLine("✅ 管理員帳號已存在");
            return;
        }

        // 建立 SuperAdmin 帳號
        var adminUsername = "admin";
        var adminEmail = "admin@localhost.com";
        var adminPassword = "Admin123!";
        var adminDisplayName = "系統管理員";

        var success = await adminService.CreateAdminAsync(
            adminUsername,
            adminEmail,
            adminPassword,
            adminDisplayName,
            "SuperAdmin"
        );

        if (success)
        {
            Console.WriteLine("🎉 預設管理員帳號已建立:");
            Console.WriteLine($"📧 Email: {adminEmail}");
            Console.WriteLine($"🔐 密碼: {adminPassword}");
            Console.WriteLine($"🌐 登入頁面: /User/Account/Login (統一登入)");
            Console.WriteLine($"⚠️  請登入後立即更改密碼！");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("❌ 預設管理員帳號建立失敗");
        }

        // 建立一般管理員帳號
        var existingManager = await adminService.GetAdminByUsernameAsync("manager");
        if (existingManager == null)
        {
            var managerSuccess = await adminService.CreateAdminAsync(
                "manager",
                "manager@localhost.com",
                "Manager123!",
                "店長",
                "Admin"
            );

            if (managerSuccess)
            {
                Console.WriteLine("👤 一般管理員帳號已建立:");
                Console.WriteLine($"📧 Email: manager@localhost.com");
                Console.WriteLine($"🔐 密碼: Manager123!");
                Console.WriteLine();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 建立管理員帳號時發生錯誤: {ex.Message}");
        Console.WriteLine($"詳細錯誤: {ex.StackTrace}");
    }
}