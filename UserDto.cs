// 用於註冊與登入的資料接收物件（DTO）
public class UserDto
{
    public required string Username { get; set; } // 使用者帳號（必填）
    public required string Password { get; set; } // 使用者密碼（必填）
}
