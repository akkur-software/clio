using Clio.Helpers;
using System.Windows;
using System.Windows.Media;

namespace Clio;

/// <summary>
/// Класс приложения.
/// </summary>
public partial class App : Application
{
    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        var topColor = (Color)ColorConverter.ConvertFromString("#0A0C10");
        var bottomColor = (Color)ColorConverter.ConvertFromString("#2E4260");
        var overlayBitmap = ThemeHelper.GenerateGradientWithDithering(topColor, bottomColor, 0.95);

        var overlayBrush = new ImageBrush(overlayBitmap)
        {
            Stretch = Stretch.UniformToFill
        };

        overlayBrush.Freeze();
        Application.Current.Resources["ModalOverlayBackground"] = overlayBrush;

        base.OnStartup(e);
    }
}
