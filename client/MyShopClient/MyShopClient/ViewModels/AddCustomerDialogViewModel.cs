using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Add/Edit Customer Dialog
/// </summary>
public partial class AddCustomerDialogViewModel : ObservableObject
{
    // Edit mode
    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _customerId;

    [ObservableProperty]
    private string _dialogTitle = "Add New Customer";

    // Form fields
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    // Validation
    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    // Events
    public event EventHandler<bool>? DialogCloseRequested;

    public AddCustomerDialogViewModel()
    {
    }

    /// <summary>
    /// Load customer data for editing from list
    /// </summary>
    public void LoadCustomer(CustomerViewModel customer)
    {
        IsEditMode = true;
        DialogTitle = $"Edit Customer";
        CustomerId = customer.Id;
        Name = customer.Name;
        Email = customer.Email ?? string.Empty;
        Phone = customer.Phone ?? string.Empty;
        Address = customer.Address ?? string.Empty;
    }

    /// <summary>
    /// Load customer data for editing from detail view
    /// </summary>
    public void LoadFromDetailViewModel(CustomerDetailViewModel detail)
    {
        IsEditMode = true;
        DialogTitle = $"Edit Customer";
        CustomerId = detail.Id;
        Name = detail.Name;
        Email = detail.Email ?? string.Empty;
        Phone = detail.Phone ?? string.Empty;
        Address = detail.Address ?? string.Empty;
    }

    [RelayCommand]
    private void Save()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(Name))
        {
            HasErrors = true;
            ErrorMessage = "Customer name is required";
            return;
        }

        HasErrors = false;
        ErrorMessage = string.Empty;
        IsSaving = true;

        // TODO: Save customer to API

        IsSaving = false;
        DialogCloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogCloseRequested?.Invoke(this, false);
    }
}
