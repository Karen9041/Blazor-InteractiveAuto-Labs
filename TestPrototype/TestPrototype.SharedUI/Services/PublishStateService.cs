namespace TestPrototype.SharedUI.Services;
public class PublishStateService
{
    public bool IsOpen { get; private set; }

    public event Action? OnChange;

    public void Open()
    {
        IsOpen = true;
        NotifyStateChanged();
    }

    public void Close()
    {
        IsOpen = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}