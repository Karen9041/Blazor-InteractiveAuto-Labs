namespace TestPrototype.SharedUI.Models
{
    public class PostDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AuthorName { get; set; } = "";
        public string AuthorHandle { get; set; } = "";
        public string? AuthorAvatarUrl { get; set; }
        public string Content { get; set; } = "";
        public string? ImageUrl { get; set; }
        public DateTime PostedTime { get; set; } = DateTime.Now;
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public string? Category { get; set; }
        public string? MembershipRole { get; set; }

        public List<string> Achievements { get; set; } = new();
        public bool AuthorHasNewUpdate { get; set; }
        public ActivityRecordDto? ActivityData { get; set; }
    }
}