using Microsoft.Extensions.DependencyInjection;
using MyShopClient.ViewModels;
using MyShopClient.Services.Api;
using MyShopClient.Services.Config;
using MyShopClient.Services.Auth;

namespace MyShopClient.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<AddProductViewModel>();
        services.AddTransient<AddCategoryDialogViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<OrderDetailViewModel>();
        services.AddTransient<CreateOrderDialogViewModel>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<CustomerDetailViewModel>();
        services.AddTransient<AddCustomerDialogViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ServerConfigViewModel>();
        services.AddTransient<ReportViewModel>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register API Services as Singletons
        services.AddSingleton(AuthApiService.Instance);
        services.AddSingleton(DashboardApiService.Instance);
        services.AddSingleton(ProductApiService.Instance);
        services.AddSingleton<IReportApiService>(ReportApiService.Instance);
        services.AddTransient<Services.Import.ImportService>();
        services.AddSingleton<Services.Navigation.INavigationService, Services.Navigation.NavigationService>();
        services.AddSingleton<ServerConfigService>();
        services.AddSingleton<CredentialService>();
        services.AddSingleton<AppSettingsService>();
        
        return services;
    }
}
