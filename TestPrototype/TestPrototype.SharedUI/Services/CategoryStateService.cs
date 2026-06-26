using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services;

public class CategoryStateService: IDisposable
{
    private readonly ICategoryService _categoryService;
    private readonly PersistentComponentState _applicationState;
    private readonly PersistingComponentStateSubscription _subscription;

    public List<CategoryDto> Categories { get; private set; } = new();
    public string SelectedCategory { get; private set; } = "綜合大廳";

    public event Action? OnChange;

    public CategoryStateService(ICategoryService categoryService, PersistentComponentState applicationState)
    {
        _categoryService = categoryService;
        _applicationState = applicationState;

        // 註冊：SSR 結束時打包資料
        _subscription = _applicationState.RegisterOnPersisting(PersistData, RenderMode.InteractiveAuto);

        // 啟動時檢查：是否有 SSR 遺產？
        if (_applicationState.TryTakeFromJson<List<CategoryDto>>("category_data", out var restored))
        {
            Categories = restored!;
        }
    }

    public async Task EnsureCategoriesLoadedAsync()
    {
        // 如果已經有資料（來自 SSR 遺產或剛剛抓過），瞬間回傳
        if (Categories.Any()) return;

        Categories = await _categoryService.GetCategoriesAsync();
        NotifyStateChanged();
    }

    public void SelectCategory(string category)
    {
        if (SelectedCategory != category)
        {
            SelectedCategory = category;
            NotifyStateChanged();
        }
    }

    private Task PersistData()
    {
        if (Categories.Any())
        {
            _applicationState.PersistAsJson("category_data", Categories);
        }
        return Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
