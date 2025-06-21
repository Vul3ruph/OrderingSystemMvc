using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 您現有的 DbSet
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<OptionItem> OptionItems { get; set; }
        public DbSet<MenuItemOption> MenuItemOptions { get; set; }
        public DbSet<OrderOptionItem> OrderOptionItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }

        // 新增管理員權限
        public DbSet<AdminPermissions> AdminPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 您現有的模型配置
            // 中介表：MenuItemOption 多對多
            builder.Entity<MenuItemOption>()
                .HasKey(mio => mio.Id); // 或使用複合主鍵 HasKey(mio => new { mio.MenuItemId, mio.OptionId });

            builder.Entity<MenuItemOption>()
                .HasOne(mio => mio.MenuItem)
                .WithMany(mi => mi.MenuItemOptions)
                .HasForeignKey(mio => mio.MenuItemId);

            builder.Entity<MenuItemOption>()
                .HasOne(mio => mio.Option)
                .WithMany() // 若 Option 不需要回導航屬性就這樣
                .HasForeignKey(mio => mio.OptionId);

            // Option 與 OptionItem：一對多
            builder.Entity<OptionItem>()
                .HasOne(oi => oi.Option)
                .WithMany(o => o.OptionItems)
                .HasForeignKey(oi => oi.OptionId);

            // 新增：管理員權限關聯配置
            builder.Entity<AdminPermissions>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<AdminPermissions>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 新增：為 ApplicationUser 添加索引
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserType)
                .HasDatabaseName("IX_AspNetUsers_UserType");

            // 可選：為管理員權限表添加索引
            builder.Entity<AdminPermissions>()
                .HasIndex(p => p.UserId)
                .IsUnique()
                .HasDatabaseName("IX_AdminPermissions_UserId");
        }
    }
}