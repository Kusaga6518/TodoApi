using Microsoft.AspNetCore.Authorization; // 提供角色授權功能
using Microsoft.AspNetCore.Mvc; // 提供 API 控制器功能
using Microsoft.EntityFrameworkCore; // 提供資料庫查詢與關聯載入功能
using TodoApi.Data; // 引入資料庫上下文類別 ApplicationDbContext
using TodoApi.Models; // 引入資料模型類別 User 與 Todo

[ApiController]
[Route("api/[controller]")] // 設定路由為 /api/admin
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context; // 注入資料庫上下文物件

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 管理者專屬 API 用來取得所有使用者的待辦事項
    // 只有角色為 Admin 的使用者可以存取此 API
    [HttpGet("all-todos")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTodos()
    {
        // 查詢所有待辦事項 並載入對應的使用者資料
        var todos = await _context.Todos
            .Include(t => t.User) // 使用 Include 載入 User 導覽屬性
            .Select(t => new
            {
                t.Id, // 待辦事項編號
                Username = t.User.Username, // 使用者帳號
                t.Title, // 待辦事項標題
                t.IsCompleted, // 是否已完成
                t.CreatedAt // 建立時間
            })
            .ToListAsync();

        // 回傳統一格式的成功訊息與資料
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = todos,
            Message = "已取得所有使用者的待辦事項"
        });
    }
}
