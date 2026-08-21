using System.Text;

namespace Clio.Helpers;

/// <summary>
/// Валидатор поля ввода номера телефона
/// </summary>
public class MaskedPhoneBehavior : TextBoxValidationBehaviorBase
{
    /// <inheritdoc/>
    protected override void FormatText(string rawText, StringBuilder sb)
    {
        string digits = new string(rawText.Where(char.IsDigit).ToArray());

        if (digits.Length > 10)
        {
            digits = digits.Substring(0, 10);
        }

        if (digits.Length > 0)
        {
            sb.Append(digits);
        }
    }
}
