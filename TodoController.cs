using Microsoft.AspNetCore.Authorization; // 提供 [Authorize] 屬性與角色驗證
using Microsoft.AspNetCore.Mvc; // 支援 API 控制器與路由
using Microsoft.EntityFrameworkCore; // 支援非同步資料庫操作
using System.Security.Claims; // 用來取得 JWT 中的使用者 Id 與角色
using TodoApi.Models; // 引入資料模型（User、Todo、ApiResponse）
using TodoApi.Data; // 引入資料庫上下文類別

[ApiController]
[Route("api/[controller]")] // 路由為 /api/todo
[Authorize] // 所有方法需通過 JWT 驗證
public class TodoController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TodoController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>查詢 Todo 清單，依角色決定查詢範圍</summary>
    /// <returns>回傳目前使用者或所有使用者的 Todo 清單</returns>
    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        // 安全取得使用者 Id 的 Claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        // 安全取得角色資訊，若為 null 則預設為 "User"
        var userRoleClaim = User.FindFirst(ClaimTypes.Role);
        var userRole = userRoleClaim?.Value ?? "User";

        if (userRole == "Admin")
        {
            var todos = await _context.Todos.Include(t => t.User).ToListAsync();
            return Ok(new ApiResponse<List<Todo>>
            {
                Success = true,
                Data = todos,
                Message = "取得所有使用者的待辦事項"
            });
        }
        else
        {
            var todos = await _context.Todos
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(new ApiResponse<List<Todo>>
            {
                Success = true,
                Data = todos,
                Message = "取得自己的待辦事項"
            });
        }
    }

    /// <summary>建立新的 Todo 資料</summary>
    /// <param name="todo">待建立的 Todo 資料</param>
    /// <returns>回傳新增後的 Todo 資料</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTodo([FromBody] Todo todo)
    {
        // 安全取得使用者 Id 的 Claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        todo.UserId = userId;

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<Todo>
        {
            Success = true,
            Data = todo,
            Message = "新增成功"
        });
    }

    /// <summary>更新指定 Todo 資料</summary>
    /// <param name="id">Todo 的 Id</param>
    /// <param name="updated">更新後的 Todo 資料</param>
    /// <returns>回傳更新後的 Todo 資料</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] Todo updated)
    {
        // 安全取得使用者 Id 的 Claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new ApiResponse<Todo>
            {
                Success = false,
                Data = null,
                Message = "找不到指定的待辦事項"
            });
        }

        if (todo.UserId != userId)
        {
            return Forbid();
        }

        todo.Title = updated.Title;
        todo.IsCompleted = updated.IsCompleted;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<Todo>
        {
            Success = true,
            Data = todo,
            Message = "更新成功"
        });
    }

    /// <summary>刪除指定 Todo 資料</summary>
    /// <param name="id">Todo 的 Id</param>
    /// <returns>回傳刪除成功的 Todo 資料</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        // 安全取得使用者 Id 的 Claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        var userId = int.Parse(userIdClaim.Value);

        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound(new ApiResponse<Todo>
            {
                Success = false,
                Data = null,
                Message = "找不到指定的待辦事項"
            });
        }

        if (todo.UserId != userId)
        {
            return Forbid();
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<Todo>
        {
            Success = true,
            Data = todo,
            Message = "刪除成功"
        });
    }
}
