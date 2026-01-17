using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using MyShopClient.Infrastructure.DependencyInjection;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShopClient
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private Frame? _rootFrame;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }
        public Window? MainWindow => _window;
        public Frame? RootFrame => _rootFrame;
        public Frame? ContentFrame { get; set; }

        /// <summary>
        /// Current logged-in user's role (ADMIN or SALE)
        /// </summary>
        public string CurrentUserRole { get; set; } = string.Empty;

        /// <summary>
        /// Helper property to check if current user is ADMIN
        /// </summary>
        public bool IsAdmin => CurrentUserRole == "ADMIN";

        public App()
        {
            // Initialize base URL from saved settings before configuring services
            BaseApiService.InitializeBaseUrl();
            
            Services = ConfigureServices();
            InitializeComponent();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            services.AddViewModels();
            services.AddServices();

            return services.BuildServiceProvider();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _rootFrame = new Frame();
            _window.Content = _rootFrame;
            
            // Hiển thị window ngay lập tức
            _window.Activate();

            // Thử auto-login với credentials đã lưu
            var loginViewModel = Services.GetService<LoginViewModel>();
            if (loginViewModel != null && BaseApiService.IsConfigured)
            {
                try
                {
                    var autoLoginSuccess = await loginViewModel.TryAutoLoginAsync();
                    if (autoLoginSuccess)
                    {
                        // Check license status sau khi login
                        var licenseService = Services.GetService<LicenseApiService>();
                        if (licenseService != null)
                        {
                            try
                            {
                                var licenseStatus = await licenseService.GetStatusAsync();
                                if (licenseStatus != null && !licenseStatus.IsValid)
                                {
                                    // Trial hết và chưa kích hoạt -> redirect đến ActivationView
                                    _rootFrame.Navigate(typeof(Views.License.ActivationView));
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"License check error: {ex.Message}");
                                // Nếu không check được license, cho phép vào app
                            }
                        }
                        
                        // Auto-login thành công và license OK, navigate thẳng tới ShellPage
                        _rootFrame.Navigate(typeof(Views.ShellPage));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Auto-login error: {ex.Message}");
                }
            }

            // Nếu auto-login không thành công, hiển thị LoginView
            _rootFrame.Navigate(typeof(Views.Login.LoginView));
        }
    }
}
