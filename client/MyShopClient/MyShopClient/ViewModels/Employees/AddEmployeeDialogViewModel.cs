using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class AddEmployeeDialogViewModel : ViewModelBase
{
    private readonly EmployeeApiService _employeeApiService;
    private int? _editingEmployeeId;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "SALE";

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _hasErrors;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _dialogTitle = "Add New Employee";

    [ObservableProperty]
    private bool _isEditMode;

    public List<string> Roles { get; } = new() { "ADMIN", "SALE" };

    public AddEmployeeDialogViewModel(EmployeeApiService employeeApiService)
    {
        _employeeApiService = employeeApiService;
    }

    public void Reset()
    {
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        SelectedRole = "SALE";
        ErrorMessage = string.Empty;
        HasErrors = false;
        IsSaving = false;
        DialogTitle = "Add New Employee";
        IsEditMode = false;
        _editingEmployeeId = null;
    }

    public void SetEditMode(User employee)
    {
        Reset();
        _editingEmployeeId = employee.Id;
        Username = employee.Username;
        SelectedRole = employee.Role;
        DialogTitle = "Edit Employee";
        IsEditMode = true;
    }

    public async Task<bool> SaveAsync()
    {
        // Validate
        if (!Validate())
        {
            return false;
        }

        IsSaving = true;

        try
        {
            if (IsEditMode && _editingEmployeeId.HasValue)
            {
                // Update existing employee
                var request = new UpdateEmployeeRequest
                {
                    Username = Username,
                    Role = SelectedRole
                };

                // Only include password if it was changed
                if (!string.IsNullOrEmpty(Password))
                {
                    request.Password = Password;
                }

                var result = await _employeeApiService.UpdateAsync(_editingEmployeeId.Value, request);
                return result != null;
            }
            else
            {
                // Create new employee
                var request = new CreateEmployeeRequest
                {
                    Username = Username,
                    Password = Password,
                    Role = SelectedRole
                };

                var result = await _employeeApiService.CreateAsync(request);
                return result != null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasErrors = true;
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool Validate()
    {
        HasErrors = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required";
            HasErrors = true;
            return false;
        }

        if (Username.Length < 3)
        {
            ErrorMessage = "Username must be at least 3 characters";
            HasErrors = true;
            return false;
        }

        // Password is required for new employees
        if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required";
            HasErrors = true;
            return false;
        }

        // If password is provided, validate it
        if (!string.IsNullOrEmpty(Password))
        {
            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                HasErrors = true;
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                HasErrors = true;
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(SelectedRole))
        {
            ErrorMessage = "Please select a role";
            HasErrors = true;
            return false;
        }

        return true;
    }
}
