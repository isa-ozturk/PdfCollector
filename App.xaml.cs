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
