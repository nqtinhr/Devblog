using DevBlog.Core.Domain.Identity;
using DevBlog.Core.Models.System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;

namespace DevBlog.Api.Extensions
{
    public static class ClaimExtensions
    {
        public static void GetPermissions(this List<RoleClaimsDto> allPermissions, Type policy)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fi in fields)
            {
                var attribute = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                string displayName = fi.GetValue(null).ToString();
                var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attributes.Length > 0)
                {
                    var description = (DescriptionAttribute)attribute[0];
                    displayName = description.Description;
                }
                // Thêm quyền mới vào danh sách các quyền với thông tin cần thiết
                allPermissions.Add(new RoleClaimsDto
                {
                    Value = fi.GetValue(null).ToString(),
                    Type = "Permissions",
                    DisplayName = displayName
                });
            }
        }
        public static async Task AddPermissionClaim(this RoleManager<AppRole> roleManager, AppRole role, string permission)
        {
            // Lấy tất cả các quyền hiện có roles
            var allClaims = await roleManager.GetClaimsAsync(role);

            // Kiểm tra xem quyền mới cần được thêm đã tồn tại chưa
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
            {
                // Nếu quyền chưa tồn tại, thêm quyền mới vào vai trò
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }
}
