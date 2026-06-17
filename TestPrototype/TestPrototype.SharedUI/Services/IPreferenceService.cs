namespace TestPrototype.SharedUI.Services
{
    public interface IPreferenceService
    {
        Task SetValueAsync(string key, string value, int expirationDays = 365);
        Task<string> GetValueAsync(string key);
    }
}
