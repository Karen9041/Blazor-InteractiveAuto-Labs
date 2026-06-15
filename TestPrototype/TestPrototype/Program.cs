using TestPrototype.Components;
using TestPrototype.SharedUI;
using TestPrototype.SharedUI.Services;
using TestPrototype.SharedUI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSharedUIServices();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ServerCookieHandler>();
builder.Services.AddHttpClient("BffClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7288"); // 改成你的伺服器網址
})
.AddHttpMessageHandler<ServerCookieHandler>(); //關鍵：裝上攔截器

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BffClient"));

// 防止未來部署至負載平衡器時遺失 Secure 標籤
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
    typeof(TestPrototype.Client._Imports).Assembly,
    typeof(TestPrototype.SharedUI.Pages.Home).Assembly
    );

//Mock API
app.MapPost("/api/mock/silent-login", (SilentLoginRequestDto req, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(req.Ticket))
    {
        return Results.BadRequest(new { message = "缺少ticket" });
    }

    context.Response.Cookies.Delete("AccessToken"); // 預防多胞胎

    var cookieOptions = new CookieOptions
    {
        HttpOnly = true, //禁止前端JS讀取
        Secure = true, //要求Https
        SameSite = SameSiteMode.Lax,
        Path = "/",
        Expires = DateTime.UtcNow.AddDays(7)
    };

    context.Response.Cookies.Delete("AccessToken");
    //cookies寫入response
    context.Response.Cookies.Append("AccessToken", $"Token_For_{req.Ticket}", cookieOptions);

    return Results.Ok(new { Message = $"靜默登入成功，歡迎 {req.Ticket}" } );
});

app.MapPost("/api/mock/login", (LoginRequestDto req, HttpContext context) =>
{
    // Demo 階段：只要有輸入帳號就當作成功，並把帳號名稱存進 Cookie 模擬真實 Token
    if (!string.IsNullOrWhiteSpace(req.Username))
    {
        context.Response.Cookies.Delete("AccessToken");
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/api/mock" });
        context.Response.Cookies.Append("AccessToken", $"Token_For_{req.Username}", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        });
        return Results.Ok(new { Message = "登入成功" });
    }
    return Results.BadRequest("請輸入帳號");
});

app.MapPost("/api/mock/logout", (HttpContext context) =>
{
    if(context.Request.Cookies.ContainsKey("AccessToken"))
    {
        context.Response.Cookies.Delete("AccessToken");
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/api/mock" });
        return Results.Ok(new { Message = "Mock Cookie 已成功刪除" });
    }
    else
    {
        return Results.BadRequest(new { Message = "沒有找到 Mock Cookie" });
    }
});

//讓前端一開網頁就來問我是誰
app.MapGet("api/mock/me", (HttpContext context) =>
{
    // 1. 嘗試讀取名為 AccessToken 的 Cookie，並把「值」存進 token 變數
    if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
    {
        // 此時 token 的值會是 "Token_For_林口車神" 或是 "Token_For_Ticket_A"

        // 2. 字串處理：把前綴 "Token_For_" 砍掉，剩下的就是真正的名字
        var actualName = token.Replace("Token_For_", "");

        // 3. 防呆檢查：如果砍掉後是空字串，一樣當作無效憑證
        if (string.IsNullOrWhiteSpace(actualName))
        {
            return Results.Unauthorized();
        }

        // 4. 動態產生物件回傳！讓前端拿到真正的名字
        return Results.Ok(new
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8), // Demo 用：隨機生一個假 ID
            Name = actualName,
            // Demo 趣味度提升：利用免費 API，根據名字產生固定的可愛大頭貼
            AvatarUrl = $"https://api.dicebear.com/7.x/adventurer/svg?seed={actualName}"
        });
    }
    else
    {
        // 如果連 Cookie 都沒有帶，回傳 401 拒絕存取
        return Results.Unauthorized();
    }
});

app.Run();
