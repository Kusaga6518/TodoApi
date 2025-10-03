namespace TodoApi.Models
{
    // 使用者資料模型，對應資料庫中的 Users 資料表
    public class User
    {
        public int Id { get; set; } // 使用者唯一識別碼（主鍵）
        public required string Username { get; set; } // 使用者帳號（必填）
        public required string PasswordHash { get; set; } // 密碼雜湊值（必填）
        public required List<Todo> Todos { get; set; } // 該使用者所建立的 Todo 清單（關聯）
        public required string Role { get; set; } // 使用者角色（例如 "User" 或 "Admin"）
    }
}
