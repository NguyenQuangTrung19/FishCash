using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class PartnerViewModel : BaseViewModel
{
    private readonly IPartnerService _partnerService;

    public ObservableCollection<Partner> Partners { get; } = new();
    public ObservableCollection<Partner> Suppliers { get; } = new();
    public ObservableCollection<Partner> Buyers { get; } = new();

    // ═══ Form ═══
    [ObservableProperty] private bool isFormVisible;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string newPartnerName = string.Empty;
    [ObservableProperty] private string newPartnerPhone = string.Empty;
    [ObservableProperty] private string newPartnerAddress = string.Empty;
    [ObservableProperty] private string newPartnerNote = string.Empty;
    [ObservableProperty] private PartnerType selectedPartnerType = PartnerType.Supplier;
    
    public ObservableCollection<string> PartnerTypeOptions { get; } = new() { "Nhà cung cấp", "Khách mua" };
    [ObservableProperty] private string selectedPartnerTypeText = "Nhà cung cấp";

    // ═══ Filter ═══
    [ObservableProperty] private string filterTypeText = "Tất cả";
    public ObservableCollection<string> FilterOptions { get; } = new() { "Tất cả", "Nhà cung cấp", "Khách mua" };

    private Partner? _editingPartner;

    public PartnerViewModel(IPartnerService partnerService)
    {
        _partnerService = partnerService;
        Title = "Đối tác";
    }

    [RelayCommand]
    public async Task LoadPartnersAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Partners.Clear();
            Suppliers.Clear();
            Buyers.Clear();

            var all = await _partnerService.GetPartnersAsync();
            foreach (var p in all)
            {
                Partners.Add(p);
                if (p.PartnerType == PartnerType.Supplier) Suppliers.Add(p);
                else Buyers.Add(p);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải đối tác: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    public IEnumerable<Partner> FilteredPartners
    {
        get
        {
            if (FilterTypeText == "Nhà cung cấp") return Suppliers;
            if (FilterTypeText == "Khách mua") return Buyers;
            return Partners;
        }
    }

    partial void OnFilterTypeTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPartners));
    }

    [RelayCommand]
    public void ToggleForm()
    {
        if (IsFormVisible)
        {
            IsFormVisible = false;
            ResetForm();
        }
        else
        {
            IsFormVisible = true;
            IsEditing = false;
            ResetForm();
        }
    }

    [RelayCommand]
    public void EditPartner(Partner partner)
    {
        if (partner == null) return;
        _editingPartner = partner;
        NewPartnerName = partner.Name;
        NewPartnerPhone = partner.Phone;
        NewPartnerAddress = partner.Address;
        NewPartnerNote = partner.Note;
        SelectedPartnerTypeText = partner.PartnerType == PartnerType.Supplier ? "Nhà cung cấp" : "Khách mua";
        IsEditing = true;
        IsFormVisible = true;
    }

    [RelayCommand]
    public async Task SavePartnerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPartnerName))
        {
            await Shell.Current.DisplayAlert("Lỗi", "Tên đối tác không được để trống", "OK");
            return;
        }

        var type = SelectedPartnerTypeText == "Nhà cung cấp" ? PartnerType.Supplier : PartnerType.Buyer;

        try
        {
            if (IsEditing && _editingPartner != null)
            {
                _editingPartner.Name = NewPartnerName;
                _editingPartner.Phone = NewPartnerPhone;
                _editingPartner.Address = NewPartnerAddress;
                _editingPartner.Note = NewPartnerNote;
                _editingPartner.PartnerType = type;
                await _partnerService.UpdatePartnerAsync(_editingPartner);
            }
            else
            {
                await _partnerService.AddPartnerAsync(new Partner
                {
                    Name = NewPartnerName,
                    Phone = NewPartnerPhone,
                    Address = NewPartnerAddress,
                    Note = NewPartnerNote,
                    PartnerType = type
                });
            }

            IsFormVisible = false;
            ResetForm();
            await LoadPartnersAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DeletePartnerAsync(Partner partner)
    {
        if (partner == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Xóa đối tác \"{partner.Name}\"?", "Xóa", "Hủy");
        if (!confirm) return;

        try
        {
            await _partnerService.DeletePartnerAsync(partner.Id);
            await LoadPartnersAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
    }

    private void ResetForm()
    {
        NewPartnerName = string.Empty;
        NewPartnerPhone = string.Empty;
        NewPartnerAddress = string.Empty;
        NewPartnerNote = string.Empty;
        SelectedPartnerTypeText = "Nhà cung cấp";
        IsEditing = false;
        _editingPartner = null;
    }
}
