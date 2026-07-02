using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestPrototype.SharedUI.Enums;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services;

public class CategoryStateService: IDisposable
{
    private const string AllCategoryName = "綜合大廳";
    private readonly ICategoryService _categoryService;
    private readonly PersistentComponentState _applicationState;
    private readonly PersistingComponentStateSubscription _subscription;
    private bool _hasHydrated;

    public UIState CurrentUIState { get; private set; } = UIState.Loading;
    public List<CategoryDto> Categories { get; private set; } = new();
    public IReadOnlyList<CategoryDto> PublishCategories => Categories
        .Where(c => c.Name != AllCategoryName)
        .ToList();
    public string SelectedCategory { get; private set; } = AllCategoryName;

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
            HydrateCategory(restored);
            _hasHydrated = true;
        }
    }

    public async Task EnsureInitialCategoriesLoadedAsync()
    {
        if (_hasHydrated)
        {
            _hasHydrated = false;
            return;
        }

        await LoadCategoriesAsync();
    }

    public async Task LoadCategoriesAsync()
    {
        try
        {
            CurrentUIState = UIState.Loading;
            NotifyStateChanged();

            Categories = await _categoryService.GetCategoriesAsync();
            CurrentUIState = Categories.Count == 0 ? UIState.Empty : UIState.Success;
        }
        catch(Exception ex)
        {
            CurrentUIState = UIState.Error;
            Console.WriteLine($"載入分類失敗: {ex.Message}");
        }
        finally
        {
            NotifyStateChanged();
        }

    }

    public void HydrateCategory(List<CategoryDto>? categories)
    {
        Categories = categories ?? new();
        CurrentUIState = Categories.Count == 0 ? UIState.Empty : UIState.Success;
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
