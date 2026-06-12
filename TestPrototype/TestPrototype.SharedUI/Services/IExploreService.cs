using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public interface IExploreService
    {
        Task<List<TrendingTopicDto>> GetTrendingTopicsAsync();
        // 這裡以後也可以加 Task<List<SearchResultDto>> SearchAsync(string keyword);
    }
}
