using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;

var builder = WebApplication.CreateBuilder(args);

// 注入服務
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();

// ✅ 建立資料庫與種子資料（放一次就好）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // ⚠️ 如果你在開發階段想要重建資料庫，改成 EnsureDeleted() + EnsureCreated()
    // db.Database.EnsureDeleted(); 
    db.Database.EnsureCreated(); // 只會建立資料庫，不會套用 migration
    DataSeeder.Seed(db);
}

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// 路由設定
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { area = "User", controller = "Home", action = "Index" });

app.Run();
