using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT 驗證中介層
using Microsoft.EntityFrameworkCore; // 資料庫操作
using Microsoft.IdentityModel.Tokens; // JWT 驗證參數
using System.Text; // 編碼工具
using TodoApi.Data; // 資料庫上下文類別
using TodoApi.Models; // 資料模型（User、Todo）
using Microsoft.OpenApi.Models; // Swagger JWT 授權所需類型

var builder = WebApplication.CreateBuilder(args);

// 設定 Kestrel 伺服器監聽 HTTPS 埠（本機 7001）
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7001, listenOptions =>
    {
        listenOptions.UseHttps(); // 啟用 HTTPS 加密通訊
    });
});

// 加入 Controller 支援
builder.Services.AddControllers();

// 加入 Swagger 文件（API 文件與測試工具）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 定義 JWT Bearer 欄位，讓 Swagger UI 顯示「Authorize」按鈕
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "請輸入 JWT Token，格式為：Bearer {token}",
        Name = "Authorization", // HTTP Header 名稱
        In = ParameterLocation.Header, // 放在 Header 中
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // 套用 JWT Bearer 權限到所有 API（讓 Swagger 自動帶入 Token）
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 加入資料庫上下文（使用 SQL Server）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 讀取 JWT 密鑰並檢查是否為 null（避免 CS8604 警告）
var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT 密鑰未設定");

// 註冊 JWT 驗證服務
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // 不驗證發行者
            ValidateAudience = false, // 不驗證接收者
            ValidateLifetime = true, // 驗證 Token 有效期限
            ValidateIssuerSigningKey = true, // 驗證簽章密鑰
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // 使用 HMAC SHA256 密鑰
        };
    });

var app = builder.Build();

// 啟用 Swagger（僅限開發模式）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // 強制使用 HTTPS

app.UseAuthentication(); // 啟用 JWT 驗證中介層
app.UseAuthorization(); // 啟用授權控制中介層

app.MapControllers(); // 啟用 Controller 路由

app.Run(); // 啟動應用程式
