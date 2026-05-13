using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyPersonalSpace.Data;
using MyPersonalSpace.Middleware;
using MyPersonalSpace.Models;
using MyPersonalSpace.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))));

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere1234567890123456"))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175", "http://localhost:5176")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 注册 HttpContextAccessor（用于操作日志获取IP）
builder.Services.AddHttpContextAccessor();

// 注册操作日志服务
builder.Services.AddScoped<OperationLogService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 在开发环境使用Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// 全局异常处理中间件 - 必须在管道最前面
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowVue");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 种子数据初始化
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.Initialize(services, userManager, roleManager);
}

app.Run("http://localhost:5000");