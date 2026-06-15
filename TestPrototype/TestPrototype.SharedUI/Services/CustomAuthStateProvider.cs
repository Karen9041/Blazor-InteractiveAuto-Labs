using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using TestPrototype.SharedUI.Models;
public class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    private UserDto? _currentUser;
    private bool _isHydrated = false;

    public CustomAuthStateProvider(HttpClient httpClient, PersistentComponentState state, ILogger<CustomAuthStateProvider> logger)
    {
        _httpClient = httpClient;
        _state = state;
        // 註冊給 Server SSR 打包使用
        _subscription = state.RegisterOnPersisting(PersistAuthState, RenderMode.InteractiveAuto);
        _logger = logger;
    }

    //核心查驗站：只看 Cookie，不問你怎麼進來的，拔掉 async
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_isHydrated)
        {
            _isHydrated = true;

            // 建立一個寬鬆的 JSON 選項
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (_state.TryTakeFromJson<UserDto>("UserInfo", out var restoredUser))
            {
                _currentUser = restoredUser;
                _logger.LogInformation("保險箱開啟成功，取得使用者：{UserName}", restoredUser.Name);
            }
            else
            {
                _logger.LogWarning("保險箱是空的或讀取失敗！準備打 API...");
            }
        }

        //如果記憶體裡有資料 (從保險箱來的，或是已經打過 API 的)，直接同步回傳
        if (_currentUser != null)
        {
            //Task.FromResult ：不切換執行緒，0 延遲，絕對不閃爍
            return Task.FromResult(BuildAuthState(_currentUser));
        }
        //如果保險箱沒東西 (例如純前端 SPA 內部跳轉)，才去呼叫非同步的 API 方法
        return FetchStateFromApiAsync();
    }

    //如果沒有遺產（例如純 SPA 跳轉），才親自去打 API
    private async Task<AuthenticationState> FetchStateFromApiAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/mock/me");
            if (response.IsSuccessStatusCode)
            {
                _currentUser = await response.Content.ReadFromJsonAsync<UserDto>();
                _logger.LogInformation("API 獲取成功: {UserName}", _currentUser?.Name);
            }
            else
            {
                _currentUser = null;
                _logger.LogWarning("API 回傳失敗或未登入");
            }
        }
        catch(Exception ex)
        {
            _currentUser = null;
            _logger.LogError(ex, "API 驗證發生例外錯誤！");
        }

        return BuildAuthState(_currentUser);
    }

    private AuthenticationState BuildAuthState(UserDto? user)
    {
        if(user != null)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty) ,
                new Claim("avatar", user.AvatarUrl ?? string.Empty)
            }, "BffAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        else
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

    }

    //Server 專屬：把查到的資料打包成名為 "UserInfo" 的 JSON 塞進 HTML
    private Task PersistAuthState()
    {
        if(_currentUser != null)
        {
            _state.PersistAsJson("UserInfo", _currentUser);
            // 這裡是在 Server 印出的，WASM 看不到
            _logger.LogInformation("已將 {UserName} 裝入保險箱", _currentUser.Name);
        }
        return Task.CompletedTask;
    }

    // 廣播大聲公：暴露給外部 (例如靜默登入) 呼叫
    public void NotifyLoginStateChanged()
    {
        // 廣播前先清空記憶體，強迫 GetAuthenticationStateAsync 去打 API 拿最新狀態
        _currentUser = null;
        _isHydrated = true; // 確保不會再去拿舊的保險箱
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
