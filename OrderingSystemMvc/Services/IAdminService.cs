// 首先建立 Services 資料夾，然後建立以下檔案：

// Services/IAdminService.cs
using OrderingSystemMvc.Models;

namespace OrderingSystemMvc.Services
{
    public interface IAdminService
    {
        Task<ApplicationUser?> GetAdminByUsernameAsync(string username);
        Task<ApplicationUser?> GetAdminByEmailAsync(string email);
        Task<ApplicationUser?> GetAdminByIdAsync(string userId);
        Task<bool> CreateAdminAsync(string username, string email, string password, string displayName, string userType = "Admin");
        Task<AdminPermissions?> GetPermissionsAsync(string userId);
        Task<bool> UpdatePermissionsAsync(AdminPermissions permissions);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<List<ApplicationUser>> GetAllAdminsAsync();
    }
}