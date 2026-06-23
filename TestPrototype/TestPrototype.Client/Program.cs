using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;
using TestPrototype.SharedUI;
using TestPrototype.SharedUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient<WasmCookieHandler>();
builder.Services.AddHttpClient("API", client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<WasmCookieHandler>();

//把加工過的 HttpClient 設為全域預設值
builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<IPreferenceService, ClientPreferenceService>();
builder.Services.AddSharedUIServices();
builder.Services.AddAuthorizationCore();

var host = builder.Build();

//讓 WASM 啟動時先去讀取 Cookie 的語系
var js = host.Services.GetRequiredService<IJSRuntime>();
// 利用我們先前寫的 js helper 去拿語言 Cookie
var cultureCookie = await js.InvokeAsync<string>("cookieHelper.get", ".AspNetCore.Culture");

string cultureName = "zh-TW"; // 預設語言

// 解析微軟 Cookie 格式 (c=en-US|uic=en-US)
if (!string.IsNullOrWhiteSpace(cultureCookie) && cultureCookie.Contains("uic="))
{
    var parts = cultureCookie.Split('|');
    var uicPart = parts.FirstOrDefault(p => p.StartsWith("uic="));
    if (uicPart != null)
    {
        cultureName = uicPart.Substring(4); // 取得 "en-US"
    }
}

// 強制將 WASM 執行緒設定為 Cookie 紀錄的語言
var culture = new CultureInfo(cultureName);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
