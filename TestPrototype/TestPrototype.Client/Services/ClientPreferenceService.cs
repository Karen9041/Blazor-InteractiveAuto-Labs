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
        // 特例：放行cookie_consent本身
        if(key != "cookie_consent")
        {
            // 如果使用者沒有明確同意 (包含還沒按、或是按了拒絕)，直接中斷寫入動作
            var hasConsent = await GetValueAsync("cookie_consent");
            if(hasConsent != "true"){
                Console.WriteLine("$\"[隱私防護] 拒絕寫入 Cookie: {key}\"");
                return;
            }
        }
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

    public async Task RemoveVauleAsync(string key)
    {
        if (OperatingSystem.IsBrowser())
        {
            await _jsRuntime.InvokeVoidAsync("cookieHelper.remove", key);
        }
    }
}
