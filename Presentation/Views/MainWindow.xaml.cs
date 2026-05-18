using System.Collections.Specialized;
using System.Windows;
using PdfCollector.Core.Interfaces;
using PdfCollector.Presentation.ViewModels;

namespace PdfCollector.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel  _vm;
    private readonly IUpdateService _updateService;
    private          UpdateWindow   _updateWindow;

    public MainWindow(MainViewModel viewModel, IUpdateService updateService)
    {
        InitializeComponent();
        DataContext    = viewModel;
        _vm            = viewModel;
        _updateService = updateService;

        ((INotifyCollectionChanged)viewModel.LogEntries).CollectionChanged +=
            (_, _) =>
            {
                if (LogList.Items.Count > 0)
                    LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
            };

        updateService.UpdateAvailable += OnUpdateAvailable;
    }

    private void OnUpdateAvailable(object sender, Core.Models.UpdateInfo info)
    {
        Dispatcher.InvokeAsync(() => ShowUpdateWindow(info));
    }

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_updateWindow?.IsVisible == true)
        {
            _updateWindow.Activate();
            return;
        }

        if (_updateService.LastUpdateInfo?.IsUpdateAvailable == true)
        {
            _updateService.NotifyCachedUpdateAvailable();
        }
        else
        {
            _ = _vm.CheckForUpdatesAsync();
        }
    }

    private void ShowUpdateWindow(Core.Models.UpdateInfo info)
    {
        if (_updateWindow?.IsVisible == true)
        {
            _updateWindow.Activate();
            return;
        }

        var vm = UpdateViewModel.FromUpdateInfo(info);
        _updateWindow = new UpdateWindow(_updateService, vm) { Owner = this };
        _updateWindow.Show();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            BtnMaximize.Content = ""; // maximize icon
        }
        else
        {
            WindowState = WindowState.Maximized;
            BtnMaximize.Content = ""; // restore icon
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnInfo_Click(object sender, RoutedEventArgs e)
    {
        InfoPopup.IsOpen = !InfoPopup.IsOpen;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _updateService.UpdateAvailable -= OnUpdateAvailable;
        base.OnClosed(e);
    }
}
