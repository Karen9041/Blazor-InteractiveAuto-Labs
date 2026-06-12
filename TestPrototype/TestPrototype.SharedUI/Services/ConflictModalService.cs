using System;

public class ConflictModalService
{
    public bool IsVisible { get; private set; }
    public string NewTicket { get; private set; } // 記錄引發衝突的新票券

    public event Action OnStateChanged;

    public void Show(string newTicket)
    {
        NewTicket = newTicket;
        IsVisible = true;
        NotifyStateChanged();
    }

    public void Hide()
    {
        IsVisible = false;
        NewTicket = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}