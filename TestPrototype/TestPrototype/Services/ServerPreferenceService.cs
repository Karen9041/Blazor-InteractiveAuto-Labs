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
        var context = _httpContextAccessor.HttpContext;
        if(context != null)
        {
            var options = new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(expireDays),
                SameSite = SameSiteMode.Lax,
                HttpOnly = false // 確保前端 JS 之後也能讀寫它
            };

            // 透過 HTTP Response Headers 將 Cookie 順利送回瀏覽器
            context.Response.Cookies.Append(key, value, options);
        }

        return Task.CompletedTask;
    }

    public Task<string?> GetValueAsync(string key)
    {
        //伺服器直接從收到的 Request 裡面翻找 Cookie
        var context = _httpContextAccessor.HttpContext;
        var value = context?.Request.Cookies[key];
        return Task.FromResult(value);
    }
}
