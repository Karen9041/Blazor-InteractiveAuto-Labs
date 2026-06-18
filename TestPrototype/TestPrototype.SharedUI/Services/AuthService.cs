using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    /*處理所有與身分驗證相關的動作（Action）*/
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly LoginModalService _loginModal;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, LoginModalService loginModal)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _loginModal = loginModal;
        }

        public async Task<bool> LoginAsync(LoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/mock/login", request);
            if (response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
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
            var response = await _httpClient.PostAsJsonAsync("api/mock/login", callBackCode);
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
    }
}
