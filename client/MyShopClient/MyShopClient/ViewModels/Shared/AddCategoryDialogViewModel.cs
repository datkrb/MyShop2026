using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyShopClient.ViewModels;

public partial class AddCategoryDialogViewModel : ObservableValidator
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [MinLength(2, ErrorMessage = "Tên danh mục phải có ít nhất 2 ký tự")]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasErrors;

    public event EventHandler<bool>? DialogCloseRequested;

    public AddCategoryDialogViewModel()
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        ValidateAllProperties();

        if (base.HasErrors)
        {
            HasErrors = true;
            var allErrors = new List<string>();
            foreach (var error in GetErrors())
            {
                if (error is ValidationResult vr && !string.IsNullOrEmpty(vr.ErrorMessage))
                {
                    allErrors.Add(vr.ErrorMessage);
                }
            }
            ErrorMessage = string.Join(Environment.NewLine, allErrors);
            return;
        }

        HasErrors = false;
        IsSaving = true;

        try
        {
            // Simulate async save operation
            await Task.Delay(300);

            // Close dialog with success
            DialogCloseRequested?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi lưu danh mục: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogCloseRequested?.Invoke(this, false);
    }

    partial void OnNameChanged(string value)
    {
        ValidateProperty(value, nameof(Name));
        UpdateHasErrors();
    }

    private void UpdateHasErrors()
    {
        HasErrors = base.HasErrors;
    }
}
