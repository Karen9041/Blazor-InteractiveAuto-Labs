namespace TestPrototype.SharedUI.Services;
using TestPrototype.SharedUI.Models;

public class PostStateService
{
    private readonly IPostApiService _postApiService;
    private readonly IAuthService _authService;

    // 核心功能：維護當前畫面上所有貼文的真實狀態
    public List<PostDto> Posts { get; private set; } = new();

    public event Action? OnChange;

    public PostStateService ( IPostApiService postApiService, IAuthService authService)
    {
        _postApiService = postApiService;
        _authService = authService;
    }

    public async Task LoadTimeLineAsync()
    {
        Posts = await _postApiService.FetchTimelineAsync();
        NotifyStateChanged();
    }

    public void HydratePosts(List<PostDto>? posts)
    {
        Posts = posts ?? new();
        NotifyStateChanged();
    }

    public async Task PublishPostAsync(PostDto newPost)
    {
        var completedPost = await _postApiService.CreatePostAsync(newPost);
        Posts.Insert(0, completedPost); //可再優化
        NotifyStateChanged();
    }

    // 處理按讚、樂觀更新與 Rollback 機制
    public async Task ToggleLikeAsync(string postId)
    {
        Console.WriteLine($"click like id:{postId}");
        // 檢查權限
        if (!await _authService.RequireLoginAsync()) return;
        Console.WriteLine("auth pass");
        // 對應貼文
        var post = Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null) return;
        Console.WriteLine("post exist");

        //備份原始狀態
        bool originalIsLiked = post.IsLikedByMe;
        int originalLikeCount = post.LikeCount;

        //樂觀更新：不管 API，立刻修改狀態
        post.IsLikedByMe = !post.IsLikedByMe;
        post.LikeCount += post.IsLikedByMe ? 1 : -1;

        NotifyStateChanged();

        try
        {
            // 呼叫 API Client (純搬運工) 發送請求到後端
            bool apiSuccess = await _postApiService.ToggleLikeAsync(postId);

            if (!apiSuccess)
            {
                throw new HttpRequestException("Backend rejected the like action.");
            }
        }
        catch (Exception ex)
        {
            // Rollback：發生任何網路或伺服器異常，立刻將狀態回復原狀
            post.IsLikedByMe = originalIsLiked;
            post.LikeCount = originalLikeCount;

            // 再次廣播，UI 會自動「彈回」原本的狀態，並可選擇通知使用者
            NotifyStateChanged();

            // 這裡可以整合 Error 狀態處理，或是跳出 Toast 提示使用者「網路異常，請稍後再試」
            Console.WriteLine($"Like failed, rollbacked. Error: {ex.Message}");
        }
    }

    public async Task ToggleShareAsync(string postId)
    {

    }


    private void NotifyStateChanged() => OnChange?.Invoke();
}
