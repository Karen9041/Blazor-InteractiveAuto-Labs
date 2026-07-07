using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using TestPrototype.SharedUI.Extensions;
using TestPrototype.SharedUI.Models;
using TestPrototype.SharedUI.Services.ModalService;

namespace TestPrototype.SharedUI.Services
{
    /*處理所有與身分驗證相關的動作（Action）*/
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly LoginModalService _loginModal;
        private readonly IPreferenceService _preferenceService;
        private readonly NavigationManager _navigationManager;

        public AuthService(
            HttpClient httpClient, 
            AuthenticationStateProvider authStateProvider, 
            LoginModalService loginModal,
            IPreferenceService preferenceService,
            NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _loginModal = loginModal;
            _preferenceService = preferenceService;
            _navigationManager = navigationManager;
        }

        public async Task<bool> LoginAsync(LoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/mock/login", request);
            if (response.IsSuccessStatusCode)
            {
                var requiresReload = false;
                //從mock api獲得使用者偏好(theme, language, etc.)，並儲存到本地端
                var meReq = await _httpClient.GetAsync("/api/mock/me");
                if(meReq.IsSuccessStatusCode)
                {
                    var userPreference = await meReq.Content.ReadFromJsonAsync<UserDto>();
                    if (userPreference != null)
                    {
                        requiresReload |= await SyncPreferenceAsync("theme", userPreference.PreferredTheme);
                        requiresReload |= await SyncPreferenceAsync(
                            ".AspNetCore.Culture", 
                            userPreference.PreferredLanguage, 
                            $"c={userPreference.PreferredLanguage}|uic={userPreference.PreferredLanguage}"
                            );
                    }
                }

                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
                if (requiresReload)
                {
                    _navigationManager.NavigateTo(_navigationManager.ToLocalizedPath(), forceLoad: true);
                }
                return true;
            }
            return false;
        }
        public async Task<bool> LoginGoogleAsync(string callBackCode)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/googleSignIn", callBackCode);
            if (response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
            }
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> LoginAppleAsync(string callBackCode)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/appleSignIn", callBackCode);
            if (response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SilentLoginAsync(SilentLoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/mock/silent-login", request);
            if (response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
                return true;
            }
            return false;
        }

        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("api/mock/logout", null);
            await _preferenceService.RemoveVauleAsync("theme");
            await _preferenceService.RemoveVauleAsync(".AspNetCore.Culture");
            _navigationManager.NavigateTo(_navigationManager.ToLocalizedPath(), forceLoad: true);
            ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
        }

        public async Task<bool> SwitchAccountAsync(SilentLoginRequestDto request)
        {
            await _httpClient.PostAsync("api/mock/logout", null);
            var response = await _httpClient.PostAsJsonAsync("api/mock/silent-login", request);
            if (response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
                return true;
            }
            return false;
        }

        public async Task<bool> RequireLoginAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                _loginModal.Show();
                return false;
            }
            return true;
        }

        // 抽取 Preference 的共用方法
        private async Task<bool> SyncPreferenceAsync(string cookieKey, string? targetValue, string? formattedCookieValue = null)
        {
            // 如果雲端沒有設定偏好，就不做事，也不需要重整
            if (string.IsNullOrEmpty(targetValue))
            {
                return false;
            }

            // 如果有傳入特定格式，就用特定格式，否則直接用 targetValue
            var expectedCookie = formattedCookieValue ?? targetValue;

            // 讀取本地目前 Cookie
            var currentCookie = await _preferenceService.GetValueAsync(cookieKey);

            // 比對並覆寫
            if (currentCookie != expectedCookie)
            {
                await _preferenceService.SetValueAsync(cookieKey, expectedCookie, 365);

                if (cookieKey == "theme" && OperatingSystem.IsBrowser())
                {
                    await _preferenceService.SetValueAsync("theme", targetValue);
                    return false; // JS 變色不需要重整
                }
                return true; // 標記為：數值有變，需要重整
            }
            return false;
        }
    }
}
