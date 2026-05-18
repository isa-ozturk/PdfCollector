using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using PdfCollector.Core.Interfaces;
using PdfCollector.Presentation.ViewModels;

namespace PdfCollector.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel       _vm;
    private readonly IUpdateService      _updateService;
    private readonly IHealthCheckService _healthService;
    private          UpdateWindow        _updateWindow;
    private          HealthCheckWindow   _healthWindow;

    public MainWindow(MainViewModel viewModel, IUpdateService updateService, IHealthCheckService healthService)
    {
        InitializeComponent();
        DataContext     = viewModel;
        _vm             = viewModel;
        _updateService  = updateService;
        _healthService  = healthService;

        ((INotifyCollectionChanged)viewModel.LogEntries).CollectionChanged +=
            (_, _) =>
            {
                // CollectionChanged handler içinde ScrollIntoView çağırmak
                // ItemsGenerator'ı bozuyor; BeginInvoke ile sonraki frame'e erteliyoruz.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (LogList.Items.Count > 0)
                        LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
                }));
            };

        updateService.UpdateAvailable       += OnUpdateAvailable;
        viewModel.SumatraPdfDownloadRequested += OnSumatraPdfDownloadRequested;
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

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
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
            ShadowBorder.Margin     = new Thickness(10);
            MaximizeIcon.Visibility = Visibility.Visible;
            RestoreIcon.Visibility  = Visibility.Collapsed;
        }
        else
        {
            WindowState = WindowState.Maximized;
            ShadowBorder.Margin     = new Thickness(0);
            MaximizeIcon.Visibility = Visibility.Collapsed;
            RestoreIcon.Visibility  = Visibility.Visible;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnSumatraPdfDownloadRequested(object sender, EventArgs e)
    {
        Dispatcher.Invoke(ShowHealthCheckWindow);
    }

    private void BtnHealth_Click(object sender, RoutedEventArgs e)
    {
        ShowHealthCheckWindow();
    }

    private void ShowHealthCheckWindow()
    {
        if (_healthWindow?.IsVisible == true)
        {
            _healthWindow.Activate();
            return;
        }

        _healthWindow = new HealthCheckWindow(_healthService) { Owner = this };
        _healthWindow.Closed += async (_, _) => await _vm.RefreshHealthAsync();
        _healthWindow.Show();
    }

    private void BtnInfo_Click(object sender, RoutedEventArgs e)
    {
        InfoPopup.IsOpen = !InfoPopup.IsOpen;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _updateService.UpdateAvailable           -= OnUpdateAvailable;
        _vm.SumatraPdfDownloadRequested          -= OnSumatraPdfDownloadRequested;
        base.OnClosed(e);
    }
}
