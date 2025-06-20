using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Models;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<OptionItem> OptionItems { get; set; }
    public DbSet<MenuItemOption> MenuItemOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
    }
}
