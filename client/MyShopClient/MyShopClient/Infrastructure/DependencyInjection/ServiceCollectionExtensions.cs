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
        services.AddTransient<PromotionViewModel>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register HttpClient
        services.AddSingleton<System.Net.Http.HttpClient>();

        // Register API Services
        services.AddSingleton<AuthApiService>();
        services.AddSingleton<DashboardApiService>();
        services.AddSingleton<ProductApiService>();
        services.AddSingleton<IReportApiService, ReportApiService>();
        services.AddSingleton<OrderApiService>();
        services.AddSingleton<CustomerApiService>();
        services.AddSingleton<PromotionApiService>();
        services.AddSingleton<LicenseApiService>();

        services.AddTransient<Services.Import.ImportService>();
        services.AddSingleton<Services.Navigation.INavigationService, Services.Navigation.NavigationService>();
        services.AddSingleton<CredentialService>();
        services.AddSingleton<AppSettingsService>();
        services.AddSingleton<Services.Local.ILocalDraftService, Services.Local.LocalDraftService>();
        
        return services;
    }
}
