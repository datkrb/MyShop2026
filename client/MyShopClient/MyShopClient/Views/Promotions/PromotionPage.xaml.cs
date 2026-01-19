using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShopClient.Models;
using MyShopClient.ViewModels;

namespace MyShopClient.Views.Promotions;

public sealed partial class PromotionPage : Page
{
    public PromotionViewModel ViewModel { get; }

    public PromotionPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<PromotionViewModel>();
        this.DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.LoadPromotions();
    }

    private void DataGrid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SelectedPromotion != null)
        {
            ViewModel.EditCommand.Execute(ViewModel.SelectedPromotion);
        }
    }

    private void PromotionsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Promotion promotion)
        {
            ViewModel.EditCommand.Execute(promotion);
        }
    }

    private async void PromotionsPagination_PageChanged(object sender, int pageNumber)
    {
        await ViewModel.GoToPageAsync(pageNumber);
    }
}
