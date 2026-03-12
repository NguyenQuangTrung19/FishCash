using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishCash.Helpers;
using FishCash.Models;
using FishCash.Services;

namespace FishCash.ViewModels;

public partial class CheckoutViewModel : BaseViewModel, IQueryAttributable
{
    private readonly PrintService _printService;
    
    [ObservableProperty]
    private Order? currentOrder;

    public string QrCodeUrl => CurrentOrder != null 
        ? AppConstants.GetQrPaymentUrl(CurrentOrder.TotalAmount, CurrentOrder.Id)
        : string.Empty;

    public CheckoutViewModel(PrintService printService)
    {
        _printService = printService;
        Title = "Thanh toán thành công";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Order", out var value) && value is Order order)
        {
            CurrentOrder = order;
            OnPropertyChanged(nameof(QrCodeUrl));
        }
    }

    [RelayCommand]
    public async Task PrintInvoiceAsync()
    {
        if (CurrentOrder == null) return;
        
        try
        {
            var path = await _printService.GenerateInvoiceFileAsync(CurrentOrder);
            await Shell.Current.DisplayAlert("In hóa đơn", $"Đã in / xuất hóa đơn thành công tại:\n{path}", "Đóng");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Lỗi in hóa đơn: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DoneAsync()
    {
        // Return to PosPage
        await Shell.Current.GoToAsync("..");
    }
}
