using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
    }
}
