using System;

namespace TodoApi.Models
{
    public class Todo
    {
        public int Id { get; set; }                  // 待辦事項編號（主鍵）
        public int UserId { get; set; }              // 所屬使用者編號（外鍵）
        public string Title { get; set; }            // 待辦事項標題
        public bool IsCompleted { get; set; }        // 是否完成
        public DateTime CreatedAt { get; set; }      // 建立時間

        public User User { get; set; }               // 導覽屬性：對應的使用者
    }
}
