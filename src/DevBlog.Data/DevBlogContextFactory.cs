using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DevBlog.Data
{
    public class DevBlogContextFactory : IDesignTimeDbContextFactory<DevBlogContext>
    {
        public DevBlogContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();
            var builder = new DbContextOptionsBuilder<DevBlogContext>();
            builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            return new DevBlogContext(builder.Options);
        }
    }
    // Add-Migration Initial
    // update-database
}
