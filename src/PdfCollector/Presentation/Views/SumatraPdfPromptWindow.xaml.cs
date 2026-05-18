using System.Windows;
using System.Windows.Input;

namespace PdfCollector.Presentation.Views;

public enum SumatraPdfPromptResult { Download, Later, Decline }

public partial class SumatraPdfPromptWindow : Window
{
    public SumatraPdfPromptResult Result { get; private set; } = SumatraPdfPromptResult.Later;

    public SumatraPdfPromptWindow()
    {
        InitializeComponent();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    private void BtnDownload_Click(object sender, RoutedEventArgs e)
    {
        Result = SumatraPdfPromptResult.Download;
        Close();
    }

    private void BtnLater_Click(object sender, RoutedEventArgs e)
    {
        Result = SumatraPdfPromptResult.Later;
        Close();
    }

    private void BtnDecline_Click(object sender, RoutedEventArgs e)
    {
        Result = SumatraPdfPromptResult.Decline;
        Close();
    }
}
