public class ServerCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 抓取使用者瀏覽器傳來的原始 Cookie
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Cookies.TryGetValue("AccessToken", out var token))
        {
            // 把 Cookie 黏到 Server 即將發出的 HttpClient 請求上
            request.Headers.Add("Cookie", $"AccessToken={token}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

