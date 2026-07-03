using TestPrototype.Components;
using TestPrototype.Extensions;
using TestPrototype.Services;
using TestPrototype.SharedUI;
using TestPrototype.SharedUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSharedUIServices();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ServerCookieHandler>();
builder.Services.AddHttpClient("BffClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7288");
})
.AddHttpMessageHandler<ServerCookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BffClient"));
builder.Services.AddHttpClient("ImageFetchClient");
builder.Services.AddScoped<SkiaSharpImageService>();
builder.Services.AddScoped<IPreferenceService, ServerPreferenceService>();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

app.UseCultureRouteRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseFrameworkAssetPathCorrection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseLocalizedPageRedirects();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(TestPrototype.Client._Imports).Assembly,
        typeof(TestPrototype.SharedUI.Pages.Home).Assembly);

app.MapMockApiEndpoints();
app.MapImageEndpoints();

app.Run();
