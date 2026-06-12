namespace TestPrototype.SharedUI.Services;

public class CategoryStateService
{
    public string SelectedCategory { get; private set; } = "綜合大廳";

    public event Action? OnChange;

    public void SelectCategory(string category)
    {
        if (SelectedCategory != category)
        {
            SelectedCategory = category;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}