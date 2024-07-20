using DevBlog.Api.Authorization;
using DevBlog.Api.Filters;
using DevBlog.Api.Services;
using DevBlog.Core.Config;
using DevBlog.Core.Domain.Identity;
using DevBlog.Core.Models.Content;
using DevBlog.Core.SeedWorks;
using DevBlog.Core.Services;
using DevBlog.Data;
using DevBlog.Data.Repositories;
using DevBlog.Data.SeedWorks;
using DevBlog.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
var CorsPolicy = "CorsPolicy";
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, builder =>
{
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins(configuration["AllowedOrigins"])
        .AllowCredentials();
}));


// --------------- Config DB Context and ASP.NET Core Identity ---------------
// Đăng ký DbContext với DevBlogContext và sử dụng SQL Server làm cơ sở dữ liệu
builder.Services.AddDbContext<DevBlogContext>(options => options.UseSqlServer(connectionString));

// Đăng ký Identity với các loại người dùng và vai trò được chỉ định (AppUser và AppRole) và sử dụng DevBlogContext đã được cấu hình làm kho lưu trữ
builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<DevBlogContext>();

// Cấu hình các tùy chọn của Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true; // Yêu cầu ít nhất một chữ số
    options.Password.RequireLowercase = true; // Yêu cầu ít nhất một chữ cái thường
    options.Password.RequireNonAlphanumeric = true; // Yêu cầu ít nhất một ký tự đặc biệt
    options.Password.RequireUppercase = true; // Yêu cầu ít nhất một chữ cái in hoa
    options.Password.RequiredLength = 6; // Yêu cầu mật khẩu có ít nhất 6 ký tự
    options.Password.RequiredUniqueChars = 1; // Yêu cầu ít nhất một ký tự đặc biệt

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa tài khoản mặc định trong 5 phút khi đăng nhập thất bại
    options.Lockout.MaxFailedAccessAttempts = 5; // Số lần đăng nhập thất bại tối đa trước khi khóa tài khoản
    options.Lockout.AllowedForNewUsers = false; // Không cho phép khóa tài khoản cho người dùng mới

    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Các ký tự cho phép trong tên người dùng
    options.User.RequireUniqueEmail = true; // Yêu cầu email phải là duy nhất
});


// Add services to the container.
builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business services and repositories
// Lấy ra tất cả các dịch vụ Repository trong assembly chứa lớp PostRepository và đăng ký chúng vào IServiceCollection
// Lọc ra các lớp có tên là IRepository<,> (interface Repository) và không phải là abstract class, không phải là generic class
var services = typeof(PostRepository).Assembly.GetTypes()
    .Where(x => x.GetInterfaces().Any(i => i.Name == typeof(IRepository<,>).Name)
        && !x.IsAbstract && x.IsClass && !x.IsGenericType);

foreach (var service in services)
{
    // Lấy tất cả các interface mà dịch vụ này thực hiện
    var allInterfaces = service.GetInterfaces();
    // Lấy ra interface trực tiếp mà dịch vụ này thực hiện, loại bỏ các interface con của các interface khác
    var directInterface = allInterfaces.Except(allInterfaces.SelectMany(t => t.GetInterfaces())).FirstOrDefault();
    if (directInterface != null)
    {
        // Đăng ký dịch vụ vào IServiceCollection với kiểu dữ liệu của interface trực tiếp và kiểu dịch vụ
        builder.Services.Add(new ServiceDescriptor(directInterface, service, ServiceLifetime.Scoped));
    }
}

//Auto mapper
builder.Services.AddAutoMapper(typeof(PostInListDto));

//Authen and author
builder.Services.Configure<JwtTokenSettings>(configuration.GetSection("JwtTokenSettings"));
builder.Services.Configure<MediaSettings>(configuration.GetSection("MediaSettings"));

builder.Services.AddScoped<SignInManager<AppUser>, SignInManager<AppUser>>();
builder.Services.AddScoped<UserManager<AppUser>, UserManager<AppUser>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<RoleManager<AppRole>, RoleManager<AppRole>>();
builder.Services.AddScoped<IRoyaltyService, RoyaltyService>();

// Serilog
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
//Default config for ASP.NET Core
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
    c.SwaggerDoc("AdminAPI", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "API for Administrators",
        Description = "API for CMS core domain. This domain keeps track of campaigns, campaign rules, and campaign execution."
    });
    // Add authorization header definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });
    c.ParameterFilter<SwaggerNullableParameterFilter>();
});

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;

    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = configuration["JwtTokenSettings:Issuer"],
        ValidAudience = configuration["JwtTokenSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtTokenSettings:Key"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("AdminAPI/swagger.json", "Admin API");
        c.DisplayOperationId();
        c.DisplayRequestDuration();
    });
}

app.UseStaticFiles();

app.UseCors(CorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//Seeding data
// app.MigrateDatabase();

app.Run();
