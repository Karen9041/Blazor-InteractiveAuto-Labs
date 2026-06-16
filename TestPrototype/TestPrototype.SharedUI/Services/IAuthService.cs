using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services
{
    public interface IAuthService
    {
        // 統一回傳 bool，代表「是否成功取得 Cookie」
        Task<bool> LoginAsync(LoginRequestDto request);
        Task<bool> SilentLoginAsync(SilentLoginRequestDto request);
        Task LogoutAsync();
        Task<bool> SwitchAccountAsync(SilentLoginRequestDto request);
        Task<bool> RequireLoginAsync();
    }
}
