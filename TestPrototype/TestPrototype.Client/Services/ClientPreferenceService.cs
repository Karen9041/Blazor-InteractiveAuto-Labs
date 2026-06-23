using Microsoft.JSInterop;
using TestPrototype.SharedUI.Services;
public class ClientPreferenceService : IPreferenceService
{
    private readonly IJSRuntime _jsRuntime;
    public ClientPreferenceService(IJSRuntime jSRuntime) {
        _jsRuntime = jSRuntime;
    }

    public async Task SetValueAsync(string key, string value, int expireDays = 365)
    {
        // 防呆：確保只有在瀏覽器環境才執行 JS，避免 SSR 渲染時報錯
        if (OperatingSystem.IsBrowser())
        {
            await _jsRuntime.InvokeVoidAsync("cookieHelper.set", key, value, expireDays);
        }
    }

    public async Task<string?> GetValueAsync(string key)
    {
        if (OperatingSystem.IsBrowser())
        {
            return await _jsRuntime.InvokeAsync<string?>("cookieHelper.get", key);
        }
        return null;
    }
}
