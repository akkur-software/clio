using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Clio.Helpers;

/// <summary>
/// Валидатор поля ввода электронной почты
/// </summary>
public class MaskedEmailBehavior : TextBoxValidationBehaviorBase
{
    /// <summary>
    /// Маска ключевых символов адреса
    /// </summary>
    private static readonly Regex KeyFilterRegex = new Regex(@"^[a-zA-Z0-9@\.\-_]+$");

    /// <summary>
    /// Структурная маска адреса
    /// </summary>
    private static readonly Regex EmailStructureRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!KeyFilterRegex.IsMatch(e.Text))
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override bool ValidateOnLostFocus(string text)
    {
        return EmailStructureRegex.IsMatch(text.Trim());
    }
}
