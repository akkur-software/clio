using System.Globalization;
using System.Windows.Data;

namespace Clio.Converters;

/// <summary>
/// Конвертер значений типа DateOnly для XAML
/// </summary>
public class DateOnlyConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateOnly date
            ? date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
            : String.Empty;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var strValue = value as string;

        if (string.IsNullOrWhiteSpace(strValue))
        {
            return null;
        }

        strValue = strValue.Trim();

        if (strValue.Length > 10)
        {
            strValue = strValue.Substring(0, 10);
        }

        if (DateOnly.TryParseExact(strValue, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
        {
            return result;
        }

        // Если пользователь ввел дичь, возвращаем специальное значение удержания, 
        // либо старое значение, чтобы не ломать модель
        return Binding.DoNothing;
    }
}
