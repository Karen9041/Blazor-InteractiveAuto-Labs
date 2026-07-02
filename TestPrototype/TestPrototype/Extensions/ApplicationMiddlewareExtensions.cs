using Microsoft.AspNetCore.Localization;
using TestPrototype.SharedUI.Extensions;

namespace TestPrototype.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static WebApplication UseCultureRouteRequestLocalization(this WebApplication app)
    {
        var supportedCultures = CultureRouteHelper.SupportedCultures;
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(CultureRouteHelper.DefaultCulture)
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        localizationOptions.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
        {
            var routeCulture = CultureRouteHelper.GetCultureFromPath(context.Request.Path.Value);
            return Task.FromResult(routeCulture is null ? null : new ProviderCultureResult(routeCulture, routeCulture));
        }));

        localizationOptions.ApplyCurrentCultureToResponseHeaders = true;

        app.UseRequestLocalization(localizationOptions);

        app.Use(async (context, next) =>
        {
            var routeCulture = CultureRouteHelper.GetCultureFromPath(context.Request.Path.Value);
            if (routeCulture is not null)
            {
                context.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(routeCulture, routeCulture)),
                    new CookieOptions
                    {
                        Path = "/",
                        SameSite = SameSiteMode.Lax,
                        Secure = context.Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddYears(1)
                    });
            }

            await next();
        });

        return app;
    }

    public static WebApplication UseFrameworkAssetPathCorrection(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value;

            if (path != null && path.Contains("/_framework/") && !path.StartsWith("/_framework/"))
            {
                context.Request.Path = path[path.IndexOf("/_framework/", StringComparison.Ordinal)..];
            }

            await next();
        });

        return app;
    }

    public static WebApplication UseLocalizedPageRedirects(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? "/";

            if (!HttpMethods.IsGet(context.Request.Method) ||
                CultureRouteHelper.GetCultureFromPath(path) is not null ||
                ShouldSkipLocalizationRedirect(path))
            {
                await next();
                return;
            }

            var userCulture = GetPreferredCulture(context);
            var localizedPath = path == "/"
                ? $"/{userCulture}/"
                : $"/{userCulture}{path}";

            context.Response.Redirect(localizedPath + context.Request.QueryString);
        });

        return app;
    }

    private static string GetPreferredCulture(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(CookieRequestCultureProvider.DefaultCookieName, out var cultureCookie))
        {
            var parsedCookie = CookieRequestCultureProvider.ParseCookieValue(cultureCookie);
            return CultureRouteHelper.NormalizeCulture(parsedCookie?.Cultures.FirstOrDefault().Value);
        }

        return CultureRouteHelper.DefaultCulture;
    }

    private static bool ShouldSkipLocalizationRedirect(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Path.HasExtension(path);
    }
}
