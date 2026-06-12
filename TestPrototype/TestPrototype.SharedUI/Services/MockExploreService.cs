using TestPrototype.SharedUI.Models;
using TestPrototype.SharedUI.Services;

public class MockExploreService : IExploreService
{
    public async Task<List<TrendingTopicDto>> GetTrendingTopicsAsync() {

        //模擬網路延遲
        await Task.Delay(800);

        return new List<TrendingTopicDto>
        {
            new TrendingTopicDto
                {
                    Rank = 1,
                    Category = "裝備請益",
                    TopicName = "推薦的室內訓練台？",
                    DiscussionCount = 142,
                    IsNew = true
                },
                new TrendingTopicDto
                {
                    Rank = 2,
                    Category = "官方活動",
                    TopicName = "週末環台虛擬騎乘賽",
                    DiscussionCount = 89,
                    IsNew = false
                }
        };
    }
}