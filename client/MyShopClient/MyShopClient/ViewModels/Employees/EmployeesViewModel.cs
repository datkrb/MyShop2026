using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShopClient.Models;
using MyShopClient.Services.Api;
using MyShopClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class EmployeesViewModel : ViewModelBase
{
    private readonly EmployeeApiService _employeeApiService;

    [ObservableProperty]
    private ObservableCollection<EmployeeViewModel> _employees = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _totalEmployees;

    [ObservableProperty]
    private int _adminCount;

    [ObservableProperty]
    private int _saleCount;

    public EmployeesViewModel(EmployeeApiService employeeApiService)
    {
        _employeeApiService = employeeApiService;
    }

    [RelayCommand]
    public async Task LoadEmployeesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var employees = await _employeeApiService.GetAllAsync();
            
            Employees.Clear();
            foreach (var emp in employees)
            {
                Employees.Add(new EmployeeViewModel(emp));
            }

            TotalEmployees = Employees.Count;
            AdminCount = Employees.Count(e => e.Role == "ADMIN");
            SaleCount = Employees.Count(e => e.Role == "SALE");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load employees: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteEmployeeAsync(int employeeId)
    {
        try
        {
            var success = await _employeeApiService.DeleteAsync(employeeId);
            if (success)
            {
                var employee = Employees.FirstOrDefault(e => e.Id == employeeId);
                if (employee != null)
                {
                    Employees.Remove(employee);
                    TotalEmployees = Employees.Count;
                    AdminCount = Employees.Count(e => e.Role == "ADMIN");
                    SaleCount = Employees.Count(e => e.Role == "SALE");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete employee: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error deleting employee: {ex}");
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadEmployeesAsync();
    }
}

/// <summary>
/// Employee ViewModel for display in list
/// </summary>
public partial class EmployeeViewModel : ObservableObject
{
    public int Id { get; }
    public string Username { get; }
    public string Role { get; }
    public DateTime? CreatedAt { get; }

    public string FormattedCreatedDate => CreatedAt?.ToString("dd/MM/yyyy") ?? "-";
    
    public string RoleDisplay => Role switch
    {
        "ADMIN" => "Administrator",
        "SALE" => "Sales Staff",
        _ => Role
    };

    public string RoleBadgeColor => Role switch
    {
        "ADMIN" => "#7C5CFC",
        "SALE" => "#10B981",
        _ => "#6B7280"
    };

    public EmployeeViewModel(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Role = user.Role;
        CreatedAt = user.CreatedAt;
    }
}
