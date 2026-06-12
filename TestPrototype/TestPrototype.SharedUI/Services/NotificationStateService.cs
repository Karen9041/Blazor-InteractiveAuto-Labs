namespace TestPrototype.SharedUI.Services
{
    public class NotificationStateService
    {
        public int UnreadCount { get; private set; }

        public event Action? OnChange;

        //模擬
        public async Task FetchInitialCountAsync()
        {
            await Task.Delay(200);
            UnreadCount = 7;
            NotifyStateChanged();
        }

        public void AddNotification()
        {
            UnreadCount++;
            NotifyStateChanged();
        }

        //使用者點開通知列 全部標記為已讀
        public void MarkAllAsRead()
        {
            if(UnreadCount > 0)
            {
                UnreadCount = 0;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
