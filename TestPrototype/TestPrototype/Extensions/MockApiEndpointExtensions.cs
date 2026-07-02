using TestPrototype.SharedUI.Models;

namespace TestPrototype.Extensions;

public static class MockApiEndpointExtensions
{
    public static WebApplication MapMockApiEndpoints(this WebApplication app)
    {
        app.MapPost("/api/mock/silent-login", (SilentLoginRequestDto req, HttpContext context) =>
        {
            if (string.IsNullOrWhiteSpace(req.Ticket))
            {
                return Results.BadRequest(new { message = "缺少ticket" });
            }

            var cookieOptions = CreateAccessTokenCookieOptions();
            context.Response.Cookies.Delete("AccessToken");
            context.Response.Cookies.Append("AccessToken", $"Token_For_{req.Ticket}", cookieOptions);

            return Results.Ok(new { Message = $"靜默登入成功，歡迎 {req.Ticket}" });
        });

        app.MapPost("/api/mock/login", (LoginRequestDto req, HttpContext context) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username))
            {
                return Results.BadRequest("請輸入帳號");
            }

            DeleteAccessTokenCookies(context);
            context.Response.Cookies.Append("AccessToken", $"Token_For_{req.Username}", CreateAccessTokenCookieOptions());

            return Results.Ok(new { Message = "登入成功" });
        });

        app.MapPost("/api/mock/logout", (HttpContext context) =>
        {
            if (!context.Request.Cookies.ContainsKey("AccessToken"))
            {
                return Results.BadRequest(new { Message = "沒有找到 Mock Cookie" });
            }

            DeleteAccessTokenCookies(context);
            return Results.Ok(new { Message = "Mock Cookie 已成功刪除" });
        });

        app.MapGet("/api/mock/me", (HttpContext context) =>
        {
            if (!context.Request.Cookies.TryGetValue("AccessToken", out var token))
            {
                return Results.Unauthorized();
            }

            var actualName = token.Replace("Token_For_", "");
            if (string.IsNullOrWhiteSpace(actualName))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                Id = Guid.NewGuid().ToString()[..8],
                Name = actualName,
                AvatarUrl = $"https://api.dicebear.com/7.x/adventurer/svg?seed={actualName}",
                PreferredLanguage = "en-US",
                PreferredTheme = "dark"
            });
        });

        return app;
    }

    private static CookieOptions CreateAccessTokenCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        };
    }

    private static void DeleteAccessTokenCookies(HttpContext context)
    {
        context.Response.Cookies.Delete("AccessToken");
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
        context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/api/mock" });
    }
}
