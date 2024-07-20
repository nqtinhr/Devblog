using DevBlog.Data;
using Microsoft.EntityFrameworkCore;

namespace DevBlog.Api
{
    // Phương thức mở rộng MigrateDatabase được sử dụng để thực hiện quá trình migration của cơ sở dữ liệu
    public static class MigrationManager
    {
        // Tạo một phạm vi (scope) để quản lý các dịch vụ được cung cấp
        public static WebApplication MigrateDatabase(this WebApplication app)
        {

            using (var scope = app.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<DevBlogContext>())
                {
                    // Sử dụng phương thức Migrate() của DatabaseContext để áp dụng các migration
                    context.Database.Migrate();

                    // Tạo một đối tượng DataSeeder để thực hiện seeding dữ liệu
                    new DataSeeder().SeedAsync(context).Wait();
                }
            }
            return app;
        }
    }
}
