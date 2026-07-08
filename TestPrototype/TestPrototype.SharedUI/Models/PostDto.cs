namespace TestPrototype.SharedUI.Models
{
    public class PostDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AuthorId { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string AuthorHandle { get; set; } = "";
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; } = "";
        public string? Base64Image { get; set;}

        /// <summary>貼文中的標籤，通常是以 # 開頭的字串，例如 #CSharp、#Blazor 等。</summary>
        public List<string> Tags { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime PostedTime { get; set; } = DateTime.Now;
        public bool IsLikedByMe { get; set; } = false;
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public string? Category { get; set; }
        public string? MembershipRole { get; set; }

        public List<string> Achievements { get; set; } = new();
        public bool AuthorHasNewUpdate { get; set; }
        public ActivityRecordDto? ActivityData { get; set; }
    }

    public class PostRequest
    {
        public string UserId { get; set; }
        public string? Content { get; set; } = null;

        // 傳遞上傳到 GCS 暫存區的 ObjectName，後端據此從 GCS Temp 區抓圖下來進行 SkiaSharp 壓縮
        public string? TempObjectName { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        // 額外的自訂欄位，後端可依需求進行處理
        public Dictionary<string, string>? ExtraValues { get; set; }
    }

    public class PostResponse
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string>? Tags { get; set; }
        public string FinalImageUrl { get; set; } = "";
        public bool IsSuccess { get; set; }
    }
}
