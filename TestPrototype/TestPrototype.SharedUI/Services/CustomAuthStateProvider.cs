using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using TestPrototype.SharedUI.Models;
public class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;

    private UserDto? _currentUser;
    private bool _isHydrated = false;

    public CustomAuthStateProvider(HttpClient httpClient, PersistentComponentState state)
    {
        _httpClient = httpClient;
        _state = state;
        // 註冊給 Server SSR 打包使用
        _subscription = state.RegisterOnPersisting(PersistAuthState, RenderMode.InteractiveAuto);
    }

    //核心查驗站：只看 Cookie，不問你怎麼進來的，拔掉 async
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_isHydrated)
        {
            _isHydrated = true;
            Console.WriteLine("[AuthProvider] 正在嘗試打開保險箱...");
            if (_state.TryTakeFromJson<UserDto>("UserInfo", out var restoredUser))
            {
                _currentUser = restoredUser;
                Console.WriteLine($"[AuthProvider] 保險箱開啟成功！獲得使用者: {_currentUser?.Name}");
            }
            else
            {
                Console.WriteLine("[AuthProvider] 保險箱是空的或讀取失敗！準備打 API...");
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
                Console.WriteLine($"[AuthProvider] API 獲取成功: {_currentUser?.Name}");
            }
            else
            {
                _currentUser = null;
                Console.WriteLine("[AuthProvider] API 回傳失敗或未登入");
            }
        }
        catch(Exception ex)
        {
            _currentUser = null;
            Console.WriteLine($"[AuthProvider] API 發生例外錯誤: {ex.Message}");
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
            Console.WriteLine($"[Server AuthProvider] 已將 {_currentUser.Name} 裝入保險箱");
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
