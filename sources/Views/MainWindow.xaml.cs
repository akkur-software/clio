using Clio.Helpers;
using Clio.ViewModels;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clio.Views;

/// <summary>
/// Класс главного окна приложения.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Функция Win#@ для принудительного освобождения фокуса окна.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    /// <summary>
    /// Функция Win32 для отправки системных сообщений.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    /// <summary>
    /// Код системного Win32 сообщения о нажатии левой кнопки мыши.
    /// </summary>
    private const int WM_NCLBUTTONDOWN = 0x00A1;

    /// <summary>
    /// Предоставляет новый экземпляр <see cref="MainWindow"/>.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        Color topColor = (Color)ColorConverter.ConvertFromString("#0A0C10");
        Color bottomColor = (Color)ColorConverter.ConvertFromString("#2E4260");
        BackgroundBrush.ImageSource = ThemeHelper.GenerateGradientWithDithering(topColor, bottomColor, 1.0);

        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseButton_Click));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeButton_Click));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeButton_Click));

        DataContext = new MainViewModel();
    }

    /// <summary>
    /// Обрабатывает растяжение окна за рамку средствами Win32 API, 
    /// в обход WindowChrome и WDM.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void ResizeSide_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed && sender is Border border)
        {
            ReleaseCapture();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // Превращаем текстовый тег стороны в системный код Win32 направления растягивания
            var hitTestCode = border.Tag.ToString() switch
            {
                "Left" => 10, // HTLEFT
                "Right" => 11, // HTRIGHT
                "Top" => 12, // HTTOP
                "TopLeft" => 13, // HTTOPLEFT
                "TopRight" => 14, // HTTOPRIGHT
                "Bottom" => 15, // HTBOTTOM
                "BottomLeft" => 16, // HTBOTTOMLEFT
                "BottomRight" => 17, // HTBOTTOMRIGHT
                _ => 0
            };

            if (hitTestCode != 0)
            {                
                SendMessage(hwnd, WM_NCLBUTTONDOWN, hitTestCode, 0);
            }
        }
    }

    /// <summary>
    /// Обрабатывает перетаскивание окна за кастомный заголовок.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    /// <summary>
    /// Обрабатывает сворачивание окна.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Обрабатывает разворачивание и восстановление окна.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    /// <summary>
    /// Обрабатывает закрытие окна.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
