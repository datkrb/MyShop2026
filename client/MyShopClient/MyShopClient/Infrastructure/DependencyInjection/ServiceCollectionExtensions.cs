using Microsoft.Extensions.DependencyInjection;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;

namespace MyShopClient.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register API Services as Singletons
        services.AddSingleton(AuthApiService.Instance);
        services.AddSingleton(DashboardApiService.Instance);
        services.AddSingleton(ProductApiService.Instance);
        
        return services;
    }
}
