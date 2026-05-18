using System.Windows;
using PdfCollector.Core.Interfaces;
using PdfCollector.Presentation.ViewModels;

namespace PdfCollector.Presentation.Views;

public partial class UpdateWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly UpdateViewModel _vm;

    public UpdateWindow(IUpdateService updateService, UpdateViewModel vm)
    {
        _updateService = updateService;
        _vm            = vm;
        DataContext    = vm;
        InitializeComponent();
    }

    private void BtnLater_Click(object sender, RoutedEventArgs e) => Close();

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        _vm.IsDownloading   = true;
        _vm.DownloadStatus  = "Güncelleme indiriliyor...";
        await _updateService.DownloadUpdateAsync(_vm.AssetId);
        _vm.IsDownloading = false;
    }
}
