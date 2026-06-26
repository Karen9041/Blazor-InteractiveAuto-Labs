# Blazor InteractiveAuto i18n: route-first culture with cookie fallback

## Context

This project uses Blazor InteractiveAuto, so the first render can happen on the server and then continue in WebAssembly after hydration. Culture selection must therefore be consistent in both runtimes.

The target rule is:

- Route first: the first URL segment is the source of truth, for example `/zh-TW/` or `/en-US/login`.
- Cookie fallback: `.AspNetCore.Culture` is used only when the URL has no culture segment.
- Route writes cookie: when a request contains a route culture, the server writes the same value back to `.AspNetCore.Culture` so `/` can redirect to the last selected culture later.

## Main files

- `TestPrototype/Program.cs`
- `TestPrototype.Client/Program.cs`
- `TestPrototype.SharedUI/Extensions/CultureRouteHelper.cs`
- `TestPrototype.SharedUI/Extensions/NavigationExtensions.cs`
- `TestPrototype.SharedUI/Components/Common/LanguageSwitcher.razor`
- Navigation/link components in `TestPrototype.SharedUI/Components`

## Core implementation points

### 1. Shared CultureRouteHelper

`CultureRouteHelper` centralizes culture routing rules:

- Supported cultures: `en-US`, `zh-TW`
- Default culture: `zh-TW`
- Normalize culture casing through a whitelist
- Read culture from the first URL segment
- Generate culture-aware internal URLs
- Preserve query string and hash fragments

Important APIs:

```csharp
CultureRouteHelper.GetCultureFromPath(path);
nav.GetCurrentCulture();
nav.ToLocalizedPath("/login");
nav.ToLocalizedCurrentPath("en-US");
```

### 2. Server route-first RequestLocalization

`Program.cs` inserts a `CustomRequestCultureProvider` at index `0` in `RequestLocalizationOptions.RequestCultureProviders`.

This makes these requests deterministic:

- `/en-US/` => `en-US`
- `/zh-TW/login` => `zh-TW`
- `/` => fallback to cookie or default culture

The server also writes the route culture back to the standard ASP.NET Core culture cookie:

```csharp
CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(routeCulture, routeCulture))
```

This keeps the cookie as memory, not authority.

### 3. WASM startup uses the same priority

`TestPrototype.Client/Program.cs` now resolves culture in this order:

1. First URL segment
2. `.AspNetCore.Culture` cookie
3. `CultureRouteHelper.DefaultCulture`

Then it applies the culture to:

```csharp
CultureInfo.DefaultThreadCurrentCulture
CultureInfo.DefaultThreadCurrentUICulture
```

This prevents hydration drift where SSR renders one culture but WASM switches back to the stale cookie culture.

### 4. LanguageSwitcher bug fix

Before the fix, `LanguageSwitcher` calculated a new culture URL but navigated to `NavManager.Uri`, which is the old URL.

The fixed flow is:

1. Write `.AspNetCore.Culture`.
2. Build the current URL with the selected culture via `ToLocalizedCurrentPath(selectedCulture)`.
3. Navigate to that URL with `forceLoad: true` so SSR rerenders using the new culture.

### 5. Culture-aware links

Internal links should not use raw `href="/login"` or `NavigateTo("/")` once route culture is introduced.

Use:

```csharp
NavManager.ToLocalizedPath("/login")
NavManager.NavigateToLocalized("/notifications")
```

Updated areas include:

- `NavMenu`
- `BottomNav`
- `Logo`
- `Footer`
- `LoginPrompt`
- `UserProfileSummary`
- `TrendingTopicsWidget`
- `PostCard` hashtag links
- `AuthService` login/logout reloads
- Account conflict and notification navigation

## Route updates

The route table now accepts culture-aware variants for interactive pages:

- `/`
- `/{Culture:length(5)}/`
- `/login`
- `/{Culture:length(5)}/login`
- `/google-callback`
- `/{Culture:length(5)}/google-callback`
- `/app-simulator`
- `/{Culture:length(5)}/app-simulator`
- `/notification`
- `/notifications`
- `/{Culture:length(5)}/notification`
- `/{Culture:length(5)}/notifications`
- `/{Culture:length(5)}/post/{PostId}`

`PostDetail` was changed from `/{Culture}/post/{PostId}` to `/{Culture:length(5)}/post/{PostId}` to avoid catching unrelated first path segments.

## Important gotchas

### ASP.NET Core localization is not route-first by default

The default providers are query string, cookie, and accept-language. Route culture needs a custom provider if the route is supposed to be authoritative.

### InteractiveAuto needs both server and client culture setup

Fixing only the server is not enough. The WASM client can still read an old cookie during hydration and switch culture. Both entry points need the same priority rule.

### Static assets should be rooted

When pages live under `/zh-TW/`, relative assets can accidentally resolve under `/zh-TW/_content/...`. Rooted URLs are safer:

```html
<link rel="stylesheet" href="/app.css" />
<script src="/_content/TestPrototype.SharedUI/js/cookieHelper.js"></script>
```

### Query-only links must still point at the culture root

A hashtag link like `?tag=abc` is context-sensitive. On a post page, it stays on the post page. Build `/zh-TW/?tag=abc` instead for home filtering.

## Validation checklist

- `/` redirects to the cookie culture or `zh-TW` by default.
- `/en-US/` stays `en-US` even if the cookie was `zh-TW`.
- Route culture writes `.AspNetCore.Culture`.
- Language switching keeps the current route/query but swaps the culture segment.
- Main navigation links include the current culture segment.
- `/notification` and `/notifications` both work.
- `PostDetail` does not catch non-culture first path segments.

## Build verification

Command used:

```powershell
dotnet build TestPrototype\TestPrototype\TestPrototype.csproj -p:OutDir=D:\Blazor\TestPrototype\artifacts\verify-build\
```

Result: build succeeded with 0 errors. Existing nullable/async warnings remain outside this i18n change.