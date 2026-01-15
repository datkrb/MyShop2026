using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

/// <summary>
/// ViewModel for Add/Edit Customer Dialog
/// </summary>
public partial class AddCustomerDialogViewModel : ObservableObject
{
    private readonly CustomerApiService _customerApiService;

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

    // Individual field errors
    [ObservableProperty]
    private string _nameError = string.Empty;

    [ObservableProperty]
    private string _emailError = string.Empty;

    [ObservableProperty]
    private string _phoneError = string.Empty;

    [ObservableProperty]
    private string _addressError = string.Empty;

    // Events
    public event EventHandler<bool>? DialogCloseRequested;

    public AddCustomerDialogViewModel(CustomerApiService customerApiService)
    {
        _customerApiService = customerApiService ?? throw new ArgumentNullException(nameof(customerApiService));
    }

    /// <summary>
    /// Load customer data for editing from list
    /// </summary>
    public void LoadCustomer(CustomerViewModel customer)
    {
        IsEditMode = true;
        DialogTitle = "Edit Customer";
        CustomerId = customer.Id;
        Name = customer.Name;
        Email = customer.Email ?? string.Empty;
        Phone = customer.Phone ?? string.Empty;
        Address = customer.Address ?? string.Empty;
        ClearErrors();
    }

    /// <summary>
    /// Load customer data for editing from detail view
    /// </summary>
    public void LoadFromDetailViewModel(CustomerDetailViewModel detail)
    {
        IsEditMode = true;
        DialogTitle = "Edit Customer";
        CustomerId = detail.Id;
        Name = detail.Name;
        Email = detail.Email ?? string.Empty;
        Phone = detail.Phone ?? string.Empty;
        Address = detail.Address ?? string.Empty;
        ClearErrors();
    }

    /// <summary>
    /// Reset form for new customer
    /// </summary>
    public void Reset()
    {
        IsEditMode = false;
        DialogTitle = "Add New Customer";
        CustomerId = 0;
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
        ClearErrors();
    }

    private void ClearErrors()
    {
        HasErrors = false;
        ErrorMessage = string.Empty;
        NameError = string.Empty;
        EmailError = string.Empty;
        PhoneError = string.Empty;
        AddressError = string.Empty;
    }

    private bool Validate()
    {
        ClearErrors();
        bool isValid = true;
        var errors = new System.Collections.Generic.List<string>();

        // Name validation - required
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required";
            errors.Add(NameError);
            isValid = false;
        }
        else if (Name.Trim().Length < 2)
        {
            NameError = "Name must be at least 2 characters";
            errors.Add(NameError);
            isValid = false;
        }

        // Email validation - required and must be valid format
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Email is required";
            errors.Add(EmailError);
            isValid = false;
        }
        else
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(Email.Trim()))
            {
                EmailError = "Invalid email format";
                errors.Add(EmailError);
                isValid = false;
            }
        }

        // Phone validation - required and must be 10 or 11 digits
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = "Phone is required";
            errors.Add(PhoneError);
            isValid = false;
        }
        else
        {
            // Remove all non-digit characters to count digits
            var digitsOnly = new Regex(@"\D").Replace(Phone, "");
            if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
            {
                PhoneError = "Phone must be 10 or 11 digits";
                errors.Add(PhoneError);
                isValid = false;
            }
        }

        // Address validation - required
        if (string.IsNullOrWhiteSpace(Address))
        {
            AddressError = "Address is required";
            errors.Add(AddressError);
            isValid = false;
        }

        if (!isValid)
        {
            HasErrors = true;
            ErrorMessage = string.Join(", ", errors);
        }

        return isValid;
    }

    /// <summary>
    /// Save customer - returns true if successful, false if validation or API fails
    /// </summary>
    public async Task<bool> SaveAsync()
    {
        // Validate
        if (!Validate())
        {
            return false;
        }

        IsSaving = true;
        HasErrors = false;

        try
        {
            var customer = new Customer
            {
                Name = Name.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim()
            };

            Customer? result;

            if (IsEditMode)
            {
                result = await _customerApiService.UpdateCustomerAsync(CustomerId, customer);
            }
            else
            {
                result = await _customerApiService.CreateCustomerAsync(customer);
            }

            if (result != null)
            {
                return true;
            }
            else
            {
                HasErrors = true;
                ErrorMessage = "Failed to save customer. Please try again.";
                return false;
            }
        }
        catch (Exception ex)
        {
            HasErrors = true;
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }
}
