using System.Text.RegularExpressions;
using TestPrototype.SharedUI.Models;
using TestPrototype.SharedUI.Services;

public class MockPostApiService: IPostApiService
{
    private static List<PostDto> _mockDatabase = new List<PostDto>
    {
        new PostDto
            {
                Id = "1",
                AuthorName = "Stranger", AuthorHandle = "@@stranger_here",
                Content = "Here is a stranger passing by",
                Category = "單車專區",
                ImageUrl="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRhb7QRcwV-ifoyZgzyHtfj_7Z6hdBxxXsntg&s",
                PostedTime = DateTime.Now.AddHours(-2),
                LikeCount = 12, CommentCount = 34,
                MembershipRole = "MEMBER",
                Achievements = new List<string>{"TOWER RUNNER"},
                AuthorHasNewUpdate = true,
                ActivityData = new ActivityRecordDto
                {
                    Distance = 45.2,
                    HeartRate = 162,
                    Duration = new TimeSpan(1, 20, 0)
                }
            },
            new PostDto {
                Id = "2",
                AuthorName = "官方", AuthorHandle = "@@official",
                Content = "歡迎，開始你的運動社交生活吧！",
                Category = "官方活動", PostedTime = DateTime.Now.AddDays(-1),
                LikeCount = 99, CommentCount = 20,
                MembershipRole = "OFFICIAL"
            }
    };

    public async Task<List<PostDto>> FetchTimelineAsync()
    {
        await Task.Delay(500); // 模擬網路傳輸，測試UIState跟Skeleton顯示

        return _mockDatabase
            .OrderByDescending(p => p.Id)
            .Select(ClonePost)
            .ToList();
    }

    public event Action? OnFeedUpdated;

    public async Task<PostDto?> FetchPostByIdAsync(string postId)
    {
        await Task.Delay(300);
        var post = _mockDatabase.FirstOrDefault(p => p.Id == postId);
        return post is null ? null : ClonePost(post);
    }

    public async Task<PostDto> CreatePostAsync(PostDto newPost)
    {
        await Task.Delay(300);
        var completedPost = ClonePost(newPost);
        var maxId = _mockDatabase.Max(p => int.Parse(p.Id));
        completedPost.Id = (maxId + 1).ToString();
        _mockDatabase.Insert(0, completedPost);
        return ClonePost(completedPost); // 回傳新增完成的資料
    }

    public async Task<bool> ToggleLikeAsync(string postId)
    {
        await Task.Delay(300);
        var post = _mockDatabase.FirstOrDefault(p => p.Id == postId);
        if (post == null)
        {
            return false;
        }

        post.IsLikedByMe = !post.IsLikedByMe;
        post.LikeCount += post.IsLikedByMe ? 1 : -1;

        // 這裡單純模擬回傳成功，實際專案會依賴 HttpResponseMessage
        return true;
    }

    public async Task<string> ExecuteShareAsync(string postId)
    {
        await Task.Delay(300);
        // 在真實環境中，這裡會是帶有 Auth Token 的 HttpClient 請求
        // 模擬回傳後端產生的專屬短網址
        return $"https://localhost:7288/post/{postId}";
    }

    private static PostDto ClonePost(PostDto post)
    {
        return new PostDto
        {
            Id = post.Id,
            AuthorName = post.AuthorName,
            AuthorHandle = post.AuthorHandle,
            AuthorAvatarUrl = post.AuthorAvatarUrl,
            Content = post.Content,
            ImageUrl = post.ImageUrl,
            PostedTime = post.PostedTime,
            IsLikedByMe = post.IsLikedByMe,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            Category = post.Category,
            MembershipRole = post.MembershipRole,
            Achievements = post.Achievements.ToList(),
            AuthorHasNewUpdate = post.AuthorHasNewUpdate,
            ActivityData = post.ActivityData is null
                ? null
                : new ActivityRecordDto
                {
                    Type = post.ActivityData.Type,
                    Distance = post.ActivityData.Distance,
                    Duration = post.ActivityData.Duration,
                    HeartRate = post.ActivityData.HeartRate
                }
        };
    }
}
