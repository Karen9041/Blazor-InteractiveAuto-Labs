using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public interface IPostApiService
    {
        // 參數: 無 (或未來加上分頁參數 page, pageSize)
        Task<List<PostDto>> FetchTimelineAsync();
        Task<PostDto?> FetchPostByIdAsync(string postId);
        // 參數: 要發布的貼文資料
        // 回傳: 後端生成的完整貼文 DTO (包含真正的 ID 與發布時間)
        Task<PostDto> CreatePostAsync(PostDto newPost);
        Task<bool> ToggleLikeAsync(string postId);
        Task<string> ExecuteShareAsync(string postId);
    }
}
