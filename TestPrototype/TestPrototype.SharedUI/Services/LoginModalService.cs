namespace TestPrototype.SharedUI.Services
{
    public class LoginModalService
    {
        public bool IsVisible { get; private set; } = false;
        public event Action? OnStateChanged;

        public void Show()
        {
            IsVisible = true;
            NotifyStateChanged();
        }

        public void Hide()
        {
            IsVisible = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
