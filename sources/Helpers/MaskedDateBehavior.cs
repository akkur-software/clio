using System.Globalization;
using System.Text;

namespace Clio.Helpers;

/// <summary>
/// Валидатор поля ввода даты
/// </summary>
public class MaskedDateBehavior : TextBoxValidationBehaviorBase
{
    /// <inheritdoc/>
    protected override void FormatText(string rawText, StringBuilder sb)
    {
        var digits = new string(rawText.Where(char.IsDigit).ToArray());

        if (digits.Length > 8)
        {
            digits = digits.Substring(0, 8);
        }

        if (digits.Length > 0)
        {
            if (digits.Length <= 2)
            {
                sb.Append(digits);
            }
            else if (digits.Length <= 4)
            {
                sb.Append(digits.Substring(0, 2)).Append(".").Append(digits.Substring(2));
            }
            else
            {
                sb.Append(digits.Substring(0, 2)).Append(".")
                  .Append(digits.Substring(2, 2)).Append(".")
                  .Append(digits.Substring(4));
            }
        }
    }

    /// <inheritdoc/>
    protected override bool ValidateOnLostFocus(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return DateTime.TryParseExact(
            text.Trim(), 
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture, 
            DateTimeStyles.None, 
            out _);
    }
}
