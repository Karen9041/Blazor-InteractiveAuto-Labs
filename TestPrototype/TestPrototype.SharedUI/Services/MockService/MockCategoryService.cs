using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services.MockService
{
    public class MockCategoryService : ICategoryService
    {
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            // 模擬延遲
            await Task.Delay(2000);

            return new List<CategoryDto>
            {
                new CategoryDto { Name = "綜合大廳", Emoji = "🔥" },
                new CategoryDto { Name = "官方活動", Emoji = "🎉" },
                new CategoryDto { Name = "裝備請益", Emoji = "🛠️" },
                new CategoryDto { Name = "單車專區", Emoji = "🚴" },
                new CategoryDto { Name = "跑步討論", Emoji = "🏃" },
            };
        }
    }
}
