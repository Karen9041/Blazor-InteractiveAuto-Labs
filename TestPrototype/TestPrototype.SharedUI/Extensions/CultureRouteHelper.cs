using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace TestPrototype.SharedUI.Extensions;

public static class CultureRouteHelper
{
    public static readonly string[] SupportedCultures = ["en-US", "zh-TW"];
    public const string DefaultCulture = "zh-TW";

    public static bool IsSupportedCulture(string? culture)
    {
        return SupportedCultures.Any(supported =>
            string.Equals(supported, culture, StringComparison.OrdinalIgnoreCase));
    }

    public static string NormalizeCulture(string? culture)
    {
        return SupportedCultures.FirstOrDefault(supported =>
            string.Equals(supported, culture, StringComparison.OrdinalIgnoreCase)) ?? DefaultCulture;
    }

    public static string? GetCultureFromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var firstSegment = path.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return IsSupportedCulture(firstSegment) ? NormalizeCulture(firstSegment) : null;
    }

    public static string GetCurrentCulture(this NavigationManager nav)
    {
        var pathCulture = GetCultureFromPath(nav.ToBaseRelativePath(nav.Uri));
        return pathCulture ?? NormalizeCulture(CultureInfo.CurrentUICulture.Name);
    }

    public static string ToLocalizedPath(this NavigationManager nav, string? relativeUrl = null, string? culture = null)
    {
        var targetCulture = NormalizeCulture(culture ?? nav.GetCurrentCulture());
        var target = relativeUrl ?? string.Empty;

        if (Uri.TryCreate(target, UriKind.Absolute, out var absoluteUri) &&
            (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            return target;
        }

        if (target.StartsWith("#", StringComparison.Ordinal) ||
            target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
        {
            return target;
        }

        var queryOrHash = string.Empty;
        var queryIndex = target.IndexOfAny(['?', '#']);
        if (queryIndex == 0)
        {
            queryOrHash = target;
            target = string.Empty;
        }
        else if (queryIndex > 0)
        {
            queryOrHash = target[queryIndex..];
            target = target[..queryIndex];
        }

        var segments = target.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (segments.Count > 0 && IsSupportedCulture(segments[0]))
        {
            segments[0] = targetCulture;
        }
        else
        {
            segments.Insert(0, targetCulture);
        }

        var path = "/" + string.Join("/", segments);
        return path == $"/{targetCulture}" ? $"/{targetCulture}/{queryOrHash}" : path + queryOrHash;
    }

    public static string ToLocalizedCurrentPath(this NavigationManager nav, string culture)
    {
        return nav.ToLocalizedPath(nav.ToBaseRelativePath(nav.Uri), culture);
    }
}
