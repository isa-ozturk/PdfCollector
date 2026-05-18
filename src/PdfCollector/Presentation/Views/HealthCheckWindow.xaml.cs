using System.Windows;
using System.Windows.Input;
using PdfCollector.Core.Interfaces;
using PdfCollector.Presentation.ViewModels;

namespace PdfCollector.Presentation.Views;

public partial class HealthCheckWindow : Window
{
    public HealthCheckWindow(IHealthCheckService healthCheckService)
    {
        InitializeComponent();
        DataContext = new HealthCheckViewModel(healthCheckService);
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
