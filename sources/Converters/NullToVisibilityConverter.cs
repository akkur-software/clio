using System.Windows;
using System.Windows.Data;

namespace Clio.Converters;

/// <summary>
/// Конвертер для скрытия пустых полей в XAML
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool isInverted = parameter?.ToString() == "Inverted";
        bool isNull = value == null;

        if (isInverted)
        {
            // Если параметр Inverted: возвращаем Visible если объект ЕСТЬ, и Collapsed если объекта НЕТ
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        // По умолчанию: Visible если объекта НЕТ (для заглушки), и Collapsed если объект ЕСТЬ
        return isNull ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
