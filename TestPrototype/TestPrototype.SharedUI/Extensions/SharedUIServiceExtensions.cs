using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using TestPrototype.SharedUI.Services;

namespace TestPrototype.SharedUI
{
    public static class SharedUIServiceExtensions
    {
        public static IServiceCollection AddSharedUIServices(this IServiceCollection services)
        {
            services.AddScoped<IExploreService, MockExploreService>();
            services.AddScoped<IFeedService, MockFeedService>();
            services.AddScoped<ICategoryService, MockCategoryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddScoped<LoginModalService>();
            services.AddScoped<ConflictModalService>();

            services.AddScoped<PublishStateService>();
            services.AddScoped<CategoryStateService>();
            services.AddScoped<NotificationStateService>();

            return services;
        }
    }
}
