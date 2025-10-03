namespace TodoApi.Models
{
    // 通用 API 回傳格式模型，支援泛型資料型別
    public class ApiResponse<T>
    {
        public bool Success { get; set; } // 操作是否成功
        public T? Data { get; set; } // 回傳的資料內容（可為任意型別，可為 null）
        public string? Message { get; set; } // 操作結果訊息（成功或錯誤說明，可為 null）
    }
}