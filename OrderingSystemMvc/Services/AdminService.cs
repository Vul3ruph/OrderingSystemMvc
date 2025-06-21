// Services/AdminService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderingSystemMvc.Data;
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetAdminByUsernameAsync(string username)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == username &&
                                         (u.UserType == "Admin" || u.UserType == "SuperAdmin") &&
                                         u.IsActive);
        }

        public async Task<ApplicationUser?> GetAdminByEmailAsync(string email)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email &&
                                         (u.UserType == "Admin" || u.UserType == "SuperAdmin") &&
                                         u.IsActive);
        }

        public async Task<ApplicationUser?> GetAdminByIdAsync(string userId)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId &&
                                         (u.UserType == "Admin" || u.UserType == "SuperAdmin") &&
                                         u.IsActive);
        }

        public async Task<bool> CreateAdminAsync(string username, string email, string password, string displayName, string userType = "Admin")
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                DisplayName = displayName,
                UserType = userType,
                IsActive = true,
                EmailConfirmed = true // 管理員帳號預設確認
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            // 建立預設權限
            var permissions = new AdminPermissions
            {
                UserId = user.Id,
                CanViewOrders = true,
                CanEditOrders = userType == "SuperAdmin",
                CanDeleteOrders = userType == "SuperAdmin",
                CanViewMenuItems = true,
                CanEditMenuItems = true,
                CanDeleteMenuItems = userType == "SuperAdmin",
                CanViewCategories = true,
                CanEditCategories = true,
                CanDeleteCategories = userType == "SuperAdmin",
                CanViewReports = true,
                CanExportData = userType == "SuperAdmin",
                CanManageAdmins = userType == "SuperAdmin",
                CanManageSettings = userType == "SuperAdmin"
            };

            _context.AdminPermissions.Add(permissions);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AdminPermissions?> GetPermissionsAsync(string userId)
        {
            return await _context.AdminPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<bool> UpdatePermissionsAsync(AdminPermissions permissions)
        {
            try
            {
                _context.AdminPermissions.Update(permissions);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            var permissions = await GetPermissionsAsync(userId);
            if (permissions == null) return false;

            return permission switch
            {
                "ViewOrders" => permissions.CanViewOrders,
                "EditOrders" => permissions.CanEditOrders,
                "DeleteOrders" => permissions.CanDeleteOrders,
                "ViewMenuItems" => permissions.CanViewMenuItems,
                "EditMenuItems" => permissions.CanEditMenuItems,
                "DeleteMenuItems" => permissions.CanDeleteMenuItems,
                "ViewCategories" => permissions.CanViewCategories,
                "EditCategories" => permissions.CanEditCategories,
                "DeleteCategories" => permissions.CanDeleteCategories,
                "ViewReports" => permissions.CanViewReports,
                "ExportData" => permissions.CanExportData,
                "ManageAdmins" => permissions.CanManageAdmins,
                "ManageSettings" => permissions.CanManageSettings,
                _ => false
            };
        }

        public async Task<List<ApplicationUser>> GetAllAdminsAsync()
        {
            return await _userManager.Users
                .Where(u => u.UserType == "Admin" || u.UserType == "SuperAdmin")
                .OrderBy(u => u.DisplayName)
                .ToListAsync();
        }
    }
}