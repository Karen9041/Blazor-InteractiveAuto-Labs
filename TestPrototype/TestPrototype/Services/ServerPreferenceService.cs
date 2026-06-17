using TestPrototype.SharedUI.Services;

public class ServerPreferenceService:IPreferenceService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerPreferenceService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task SetValueAsync(string key, string value, int expireDays = 365)
    {
        //SSR 階段通常不負責「設定」偏好，只讓它回傳完成
        return Task.CompletedTask;
    }

    public Task<string> GetValueAsync(string key)
    {
        //伺服器直接從收到的 Request 裡面翻找 Cookie
        var context = _httpContextAccessor.HttpContext;
        var value = context?.Request.Cookies[key];
        return Task.FromResult(value);
    }
}
