using System.Windows;
using PdfCollector.Application.Services;
using PdfCollector.Infrastructure.Services;
using PdfCollector.Presentation.ViewModels;
using PdfCollector.Presentation.Views;

namespace PdfCollector
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (_, args) =>
            {
                var ex = args.Exception;
                System.Windows.MessageBox.Show(
                    "Beklenmeyen bir hata oluştu:\n\n" + ex.Message
                    + (ex.InnerException != null ? "\n\n" + ex.InnerException.Message : string.Empty),
                    "PdfCollector — Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                args.Handled = true;
            };

            var logService   = new LogService();
            var scanner      = new PdfScannerService();
            var zipper       = new ZipService();
            var cleanup      = new FolderCleanupService();
            var settingsSvc  = new AppSettingsService();
            var printSvc     = new PrintService();
            var updateSvc    = new UpdateService();

            var collectionSvc = new PdfCollectionService(scanner, zipper, cleanup, logService);

            var viewModel = new MainViewModel(collectionSvc, logService, settingsSvc, printSvc, updateSvc);
            var mainWindow = new MainWindow(viewModel, updateSvc);
            mainWindow.Show();
        }
    }
}
