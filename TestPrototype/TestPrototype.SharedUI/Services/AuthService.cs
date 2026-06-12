using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(LoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/mock/login", request);
            if(response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
                return true;
            }
            return false;
        }

        public async Task<bool> SilentLoginAsync(SilentLoginRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/mock/silent-login", request);
            if(response.IsSuccessStatusCode)
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
            if(response.IsSuccessStatusCode)
            {
                ((CustomAuthStateProvider)_authStateProvider).NotifyLoginStateChanged();
                return true;
            }
            return false;
        }
    }
}
