using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TestPrototype.SharedUI;
using TestPrototype.SharedUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddHttpClient("API", client =>
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<CookieHandler>();

//把加工過的 HttpClient 設為全域預設值
builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddSharedUIServices();
builder.Services.AddAuthorizationCore();


await builder.Build().RunAsync();
