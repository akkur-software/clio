using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Clio.Helpers;

/// <summary>
/// Базовый класс валидации полей ввода
/// </summary>
public abstract class TextBoxValidationBehaviorBase : Behavior<TextBox>
{
    /// <summary>
    /// Находится ли содержимое поля ввода в режиме форматирования.
    /// </summary>
    private bool _isFormatting;

    /// <summary>
    /// Оригинальная кисть рамки для границы поля ввода.
    /// </summary>
    private Brush _originalBorderBrush;

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.TextChanged += OnTextChanged;
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        AssociatedObject.LostFocus += OnLostFocus;
        AssociatedObject.GotFocus += OnGotFocus;

        ApplyFormat();
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        AssociatedObject.TextChanged -= OnTextChanged;
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.LostFocus -= OnLostFocus;
        AssociatedObject.GotFocus -= OnGotFocus;

        base.OnDetaching();
    }

    /// <summary>
    /// Предварительно обрабатывает нажатие клавиши.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs e) { }

    /// <summary>
    /// Предварительно обрабатывает ввод в поле.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    protected virtual void OnPreviewTextInput(object sender, TextCompositionEventArgs e) { }    

    /// <summary>
    /// Обрабатывает изменение текста в поле ввода.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isFormatting || AssociatedObject == null)
        {
            return;
        }
        ApplyFormat();
    }

    /// <summary>
    /// Обрабатывает потерю фокуса полем ввода.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null || string.IsNullOrWhiteSpace(AssociatedObject.Text))
        {
            return;
        }

        if (!ValidateOnLostFocus(AssociatedObject.Text))
        {
            if (_originalBorderBrush == null)
            {
                _originalBorderBrush = AssociatedObject.BorderBrush;
            }
            AssociatedObject.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B2E2E"));
        }
    }

    /// <summary>
    /// Обрабатывает получение фокуса полем ввода.
    /// </summary>
    /// <param name="sender">
    /// Инициатор события.
    /// </param>
    /// <param name="e">
    /// Аргументы события.
    /// </param>
    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject != null && _originalBorderBrush != null)
        {
            AssociatedObject.BorderBrush = _originalBorderBrush;
        }
    }

    /// <summary>
    /// Применяет форматирование к содержимому поля ввода.
    /// </summary>
    private void ApplyFormat()
    {
        _isFormatting = true;

        var input = AssociatedObject.Text ?? string.Empty;
        var selectionStart = AssociatedObject.SelectionStart;
        var oldLength = input.Length;

        var sb = new StringBuilder();

        // Вызываем логику форматирования, передавая сырой текст
        FormatText(input, sb);

        // Если StringBuilder остался пустым (как у Email), мы вообще не трогаем текст и каретку!
        if (sb.Length > 0)
        {
            var formatted = sb.ToString();
            AssociatedObject.Text = formatted;

            var newLength = formatted.Length;
            var selectionOffset = newLength - oldLength;
            var newSelectionStart = selectionStart + selectionOffset;

            AssociatedObject.SelectionStart = (newSelectionStart >= 0 && newSelectionStart <= newLength)
                ? newSelectionStart
                : newLength;
        }

        _isFormatting = false;
    }

    /// <summary>
    /// Валидирует содержимое поля ввода при потере фокуса.
    /// </summary>
    /// <param name="text">
    /// Текущее содержимое поля ввода.
    /// </param>
    /// <returns>
    /// Возвращает <see langword="true"/>, 
    /// если содержимое валидно.
    /// </returns>
    protected virtual bool ValidateOnLostFocus(string text) => true;

    /// <summary>
    /// Форматирует текст определенным образом 
    /// и сохраняет в указанный экземпляр <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="rawText">
    /// Исходный текст поля ввода.
    /// </param>
    /// <param name="sb">
    /// Экземпляр <see cref="StringBuilder"/>, 
    /// переданный извне для сохранения результатов форматирования.
    /// </param>
    protected virtual void FormatText(string rawText, StringBuilder sb) { }
}
