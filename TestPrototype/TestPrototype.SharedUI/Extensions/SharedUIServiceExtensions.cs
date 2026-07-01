using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using TestPrototype.SharedUI.Services;

namespace TestPrototype.SharedUI
{
    public static class SharedUIServiceExtensions
    {
        public static IServiceCollection AddSharedUIServices(this IServiceCollection services)
        {
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddLocalization();

            services.AddScoped<IExploreService, MockExploreService>();
            services.AddScoped<IPostApiService, MockPostApiService>();
            services.AddScoped<ICategoryService, MockCategoryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBrowserShareService, BrowserShareService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddScoped<LoginModalService>();
            services.AddScoped<ConflictModalService>();
            services.AddScoped<PublishStateService>();
            services.AddScoped<CategoryStateService>();
            services.AddScoped<NotificationStateService>();
            services.AddScoped<PostStateService>(); 

            return services;
        }
    }
}
