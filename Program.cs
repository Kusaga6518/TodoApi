using Microsoft.EntityFrameworkCore;
using TodoApi.Data;     // 如果你的 ApplicationDbContext 在 Data 資料夾
using TodoApi.Models;   // 如果你有使用 User 或 Todo 類別


var builder = WebApplication.CreateBuilder(args);

// 加入服務到 DI 容器（依賴注入容器）
// Swagger 是用來產生 API 文件與測試介面
builder.Services.AddEndpointsApiExplorer(); // 探索 API 端點（Minimal API 用）
builder.Services.AddSwaggerGen();           // 產生 Swagger 文件
builder.Services.AddControllers();          // 加入 Controller 支援（可使用 [ApiController]）

// 加入資料庫上下文（DbContext）
// 使用 SQL Server 作為資料庫提供者
// 從 appsettings.json 中讀取名為 "DefaultConnection" 的連線字串
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 設定 HTTP 請求處理流程（中介軟體）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // 啟用 Swagger 文件
    app.UseSwaggerUI();  // 啟用 Swagger UI 測試介面
}

app.UseHttpsRedirection(); // 強制使用 HTTPS
app.UseAuthorization();    // 啟用授權機制

app.MapControllers();      // 啟用 Controller 路由（對應到控制器）

// Minimal API 範例：建立 /weatherforecast 路由
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)), // 日期：今天加上 index 天
            Random.Shared.Next(-20, 55),                        // 隨機產生攝氏溫度
            summaries[Random.Shared.Next(summaries.Length)]     // 隨機選擇天氣描述
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast") // 指定路由名稱
.WithOpenApi();                 // 加入 Swagger 文件支援

app.Run(); // 啟動應用程式

// 定義資料模型：WeatherForecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556); // 計算華氏溫度
}
