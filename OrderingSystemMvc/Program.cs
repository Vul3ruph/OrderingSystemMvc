using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Helpers;
using OrderingSystemMvc.Models;

var builder = WebApplication.CreateBuilder(args);

// 注入服務
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity 設定
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 密碼設定
    options.Password.RequireDigit = false;           // 不要求數字
    options.Password.RequireLowercase = false;       // 不要求小寫
    options.Password.RequireUppercase = false;       // 不要求大寫
    options.Password.RequireNonAlphanumeric = false; // 不要求特殊字元
    options.Password.RequiredLength = 6;             // 最少 6 字元

    // 用戶設定
    options.User.RequireUniqueEmail = true;          // Email 必須唯一

    // 登入設定
    options.SignIn.RequireConfirmedEmail = false;    // 暫時不要求 Email 確認
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 設定登入路徑
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Account/Login";
    options.LogoutPath = "/User/Account/Logout";
    options.AccessDeniedPath = "/User/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // 30 天過期
    options.SlidingExpiration = true;
});

// Session 設定
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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


// Identity 認證授權
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

app.MapControllers();// 這行是為了讓 API 路由正常工作

// ✅ 建立資料庫與種子資料
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // 初始化 StaticData
    StaticData.OptionItemDict = db.OptionItems
        .Include(o => o.Option) // 如需 Option 名稱可以加
        .ToDictionary(o => o.Id);
    db.Database.EnsureCreated();  // 確保資料庫存在
    DataSeeder.Seed(db);//初始資料庫
}

app.Run();
