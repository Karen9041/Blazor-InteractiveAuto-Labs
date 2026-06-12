using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public interface IFeedService
    {
        Task<List<PostDto>> GetTimelineAsync();

        Task PublishPostAsync(PostDto newPost);
        Task<bool> LikePostAsync(string postId);

        event Action? OnFeedUpdated;
    }
}