using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace MyShopClient.ViewModels;

public partial class CustomerSelectionViewModel : ViewModelBase
{
    private readonly CustomerApiService _customerApiService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CustomerViewModel> _customers = new();

    [ObservableProperty]
    private CustomerViewModel? _selectedCustomer;

    [ObservableProperty]
    private bool _isLoading;

    // For new customer creation
    [ObservableProperty]
    private string _newCustomerName = string.Empty;

    [ObservableProperty]
    private string _newCustomerEmail = string.Empty;

    [ObservableProperty]
    private string _newCustomerPhone = string.Empty;

    [ObservableProperty]
    private string _newCustomerAddress = string.Empty;

    [ObservableProperty]
    private bool _isCreatingNew;

    public CustomerSelectionViewModel(CustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
    }

    [RelayCommand]
    public async Task Search()
    {
        IsLoading = true;
        try
        {
            // Assuming API supports search, or we filter client side if API is simple
            // For now, based on CustomersViewModel, it seems we might need to load all or search via API
            // Let's assume we can load and filter for now if API doesn't support direct search param
            // Or better, check CustomerApiService content.
            // Placeholder until I see the file content
            var result = await _customerApiService.GetCustomersAsync(1, 100, SearchQuery);
             Customers.Clear();
            if (result != null && result.Data != null)
            {
               foreach(var c in result.Data)
               {
                   Customers.Add(new CustomerViewModel
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Email = c.Email,
                       Phone = c.Phone,
                       Address = c.Address,
                       CreatedAt = c.CreatedAt
                   });
               }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreateNewCustomer()
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(NewCustomerName)) return;

        IsLoading = true;
        try
        {
            var newCustomer = new Models.Customer
            {
                Name = NewCustomerName,
                Email = NewCustomerEmail,
                Phone = NewCustomerPhone,
                Address = NewCustomerAddress
            };

            var created = await _customerApiService.CreateCustomerAsync(newCustomer);
            if (created != null)
            {
                var vm = new CustomerViewModel
                {
                    Id = created.Id,
                    Name = created.Name,
                    Email = created.Email,
                    Phone = created.Phone,
                    Address = created.Address,
                    CreatedAt = created.CreatedAt
                };
                Customers.Insert(0, vm);
                SelectedCustomer = vm;
                
                // Clear fields
                NewCustomerName = string.Empty;
                NewCustomerEmail = string.Empty;
                NewCustomerPhone = string.Empty;
                NewCustomerAddress = string.Empty;
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
