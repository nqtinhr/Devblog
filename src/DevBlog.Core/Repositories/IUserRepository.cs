using DevBlog.Core.Domain.Identity;
using DevBlog.Core.SeedWorks;

namespace DevBlog.Core.Repositories
{
    public interface IUserRepository : IRepository<AppUser, Guid>
    {
        Task RemoveUserFromRoles(Guid userId, string[] roles);
    }
}
