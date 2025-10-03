using Microsoft.AspNetCore.Mvc; // ASP.NET Core 的 MVC 功能
using Microsoft.EntityFrameworkCore; // Entity Framework Core（資料庫操作）
using Microsoft.IdentityModel.Tokens; // JWT 驗證相關類別
using System.IdentityModel.Tokens.Jwt; // JWT Token 建立與處理
using System.Security.Claims; // JWT Claims（用來描述使用者資訊）
using System.Security.Cryptography; // 密碼雜湊用的加密類別
using System.Text; // 編碼工具
using TodoApi.Models; // 資料模型命名空間（User、Todo）
using TodoApi.Data; // 資料庫上下文類別（ApplicationDbContext）

[ApiController]
[Route("api/[controller]")] // 路由為 /api/auth
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context; // 資料庫操作物件
    private readonly IConfiguration _config; // 用來讀取 appsettings 的設定值

    public AuthController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")] // 使用者註冊 API
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        // 檢查模型是否有效（避免空值）
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "請填寫完整的帳號與密碼"
            });
        }

        // 檢查密碼長度是否足夠
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "密碼長度至少需 6 字元"
            });
        }

        // 檢查使用者是否已存在
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "此帳號已被註冊"
            });
        }

        // 建立新使用者
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = HashPassword(dto.Password),
            Todos = new List<Todo>(),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Data = user.Username,
            Message = "註冊成功"
        });
    }

    [HttpPost("login")] // 使用者登入 API
    public async Task<IActionResult> Login([FromBody] UserDto dto)
    {
        // 檢查模型是否有效（避免空值）
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "請填寫帳號與密碼"
            });
        }

        // 查詢使用者（根據帳號）
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

        // 驗證密碼是否正確（比對雜湊值）
        if (user == null || user.PasswordHash != HashPassword(dto.Password))
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Data = null,
                Message = "帳號或密碼錯誤"
            });
        }

        // 登入成功，產生 JWT Token
        var token = GenerateJwtToken(user);

        // 回傳 Token 給前端（統一格式）
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Data = token,
            Message = "登入成功"
        });
    }

    private string GenerateJwtToken(User user)
    {
        // 從設定檔取得 JWT 密鑰 若未設定則丟出例外
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT 密鑰未設定");

        // 將密鑰轉成位元組陣列
        var keyBytes = Encoding.UTF8.GetBytes(key);

        // 建立簽章憑證 使用 HMAC SHA256 演算法
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        // 建立 JWT Token 包含使用者 Id 使用者名稱 與角色作為 Claims
        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 使用者唯一識別碼
                new Claim(ClaimTypes.Name, user.Username), // 使用者帳號
                new Claim(ClaimTypes.Role, user.Role) // 使用者角色（新增這一行）
            },
            expires: DateTime.UtcNow.AddHours(1), // 設定 Token 一小時後過期
            signingCredentials: creds // 使用簽章憑證簽署 Token
        );

        // 將 JWT Token 序列化為字串並回傳
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create(); // 建立 SHA256 雜湊物件
        var bytes = Encoding.UTF8.GetBytes(password); // 將密碼轉成位元組
        var hash = sha256.ComputeHash(bytes); // 計算雜湊值
        return Convert.ToBase64String(hash); // 回傳 Base64 編碼的雜湊結果
    }
}
