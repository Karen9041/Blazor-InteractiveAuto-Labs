namespace TestPrototype.SharedUI.Services
{
    public interface IBrowserShareService
    {
        Task<bool> ShareAsync(string title, string text, string url);
        Task<bool> CopyToClipboardAsync(string text);
    }
}
