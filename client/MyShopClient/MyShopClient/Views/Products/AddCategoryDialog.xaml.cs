using System;
using Microsoft.UI.Xaml.Controls;

namespace MyShopClient.Views.Products;

public sealed partial class AddCategoryDialog : ContentDialog
{
    public string CategoryName => NameBox.Text;
    public string CategoryDescription => DescriptionBox.Text;

    public AddCategoryDialog()
    {
        this.InitializeComponent();
    }
}
