using Microsoft.AspNetCore.Components;

namespace TestPrototype.SharedUI.Extensions;

public static class NavigationExtensions
{
    /// <summary>
    /// Navigate to a route with the current culture segment preserved.
    /// </summary>
    public static void NavigateToLocalized(this NavigationManager nav, string relativeUrl)
    {
        var currentCulture = nav.GetCurrentCulture();
        nav.NavigateTo(nav.ToLocalizedPath(relativeUrl.TrimStart('/'), currentCulture));
    }
}