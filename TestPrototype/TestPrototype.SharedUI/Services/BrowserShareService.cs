using Microsoft.JSInterop;

namespace TestPrototype.SharedUI.Services
{
    public class BrowserShareService : IBrowserShareService, IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

        public BrowserShareService(IJSRuntime jsRuntime)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/TestPrototype.SharedUI/js/shareHelper.js").AsTask());
        }

        public async Task<bool> ShareAsync(string title, string text, string url)
        {
            try
            {
                var module = await _moduleTask.Value;
                return await module.InvokeAsync<bool>("shareNative", title, text, url);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CopyToClipboardAsync(string text)
        {
            try
            {
                var module = await _moduleTask.Value;
                return await module.InvokeAsync<bool>("copyToClipboard", text);
            }
            catch
            {
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
