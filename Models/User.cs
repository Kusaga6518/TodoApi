namespace TodoApi.Models
{
    public class User
    {
        public int Id { get; set; }                  // 使用者編號（主鍵）
        public string Username { get; set; }         // 使用者名稱
        public string PasswordHash { get; set; }     // 密碼雜湊（加密後）
        
        public ICollection<Todo> Todos { get; set; } // 使用者擁有的待辦事項清單
    }
}
