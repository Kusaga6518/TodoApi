using System;

namespace TodoApi.Models
{
    public class Todo
    {
        public int Id { get; set; } // 待辦事項唯一識別碼
        public required string Title { get; set; } // 待辦事項標題
        public bool IsCompleted { get; set; } // 是否已完成
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 建立時間（新增這一行）
        public int UserId { get; set; } // 關聯的使用者 Id
        public required User User { get; set; } // 導覽屬性（關聯到使用者）
    }
}
