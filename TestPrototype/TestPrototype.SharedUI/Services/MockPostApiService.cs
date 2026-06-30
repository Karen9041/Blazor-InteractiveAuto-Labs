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
        await Task.Delay(2000); // 模擬網路傳輸，測試UIState跟Skeleton顯示

        return _mockDatabase.OrderByDescending(p => p.Id).ToList(); ;
    }

    public event Action? OnFeedUpdated;

    public async Task<PostDto> CreatePostAsync(PostDto newPost)
    {
        await Task.Delay(500);
        newPost.Id = _mockDatabase.Max(p => p.Id) + 1;
        _mockDatabase.Insert(0, newPost);

        return newPost; // 回傳新增完成的資料
    }

    public async Task<bool> ToggleLikeAsync(string postId)
    {
        await Task.Delay(300);
        // 這裡單純模擬回傳成功，實際專案會依賴 HttpResponseMessage
        return true;
    }
}
