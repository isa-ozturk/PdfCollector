using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using PdfCollector.Core.Interfaces;

namespace PdfCollector.Presentation.Views;

public partial class PrintWindow : Window
{
    private readonly IPrintService       _printService;
    private readonly string              _zipPath;
    private          CancellationTokenSource _cts;
    private          bool                _isPrinting;

    public PrintWindow(IPrintService printService, string zipPath)
    {
        _printService = printService;
        _zipPath      = zipPath;
        InitializeComponent();
        _ = LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        try
        {
            var count    = await _printService.GetPdfCountInZipAsync(_zipPath);
            var printers = await _printService.GetAvailablePrintersAsync();

            TxtPdfCount.Text = $"ZIP arşivinde {count} PDF dosyası bulundu.";

            CmbPrinters.Items.Clear();
            foreach (var p in printers)
                CmbPrinters.Items.Add(p);

            if (CmbPrinters.Items.Count > 0)
                CmbPrinters.SelectedIndex = 0;

            if (count == 0)
            {
                BtnPrint.IsEnabled = false;
                TxtStatus.Text = "ZIP arşivinde yazdırılacak PDF bulunamadı.";
            }
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Yüklenirken hata oluştu: {ex.Message}";
        }
    }

    private async void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
        if (_isPrinting) return;

        var selectedPrinter = CmbPrinters.SelectedItem as string ?? string.Empty;

        _isPrinting   = true;
        BtnPrint.IsEnabled   = false;
        BtnCancel.IsEnabled  = false;
        PrgPrint.Visibility  = Visibility.Visible;
        _cts = new CancellationTokenSource();

        try
        {
            var progress = new Progress<string>(msg => TxtStatus.Text = msg);
            await _printService.PrintPdfsFromZipAsync(_zipPath, selectedPrinter, progress, _cts.Token);

            BtnCancel.Content   = "Kapat";
            BtnCancel.IsEnabled = true;
        }
        catch (OperationCanceledException)
        {
            TxtStatus.Text      = "Yazdırma iptal edildi.";
            BtnCancel.IsEnabled = true;
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Hata: {ex.Message}";
            MessageBox.Show($"Yazdırma sırasında bir hata oluştu:\n{ex.Message}",
                "Yazdırma Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnPrint.IsEnabled  = true;
            BtnCancel.IsEnabled = true;
        }
        finally
        {
            _isPrinting        = false;
            PrgPrint.Visibility = Visibility.Collapsed;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (_isPrinting)
        {
            _cts?.Cancel();
        }
        else
        {
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        base.OnClosed(e);
    }
}
