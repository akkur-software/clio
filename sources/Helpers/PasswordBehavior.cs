using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Clio.Helpers;

/// <summary>
/// Поведение поля ввода пароля
/// </summary>
public class PasswordBehavior : Behavior<PasswordBox>
{
    /// <summary>
    /// Плейсхолдер
    /// </summary>
    private TextBlock _placeholder;

    /// <summary>
    /// Свойство зависимостей для хранения пароля
    /// </summary>
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), 
            typeof(string), typeof(PasswordBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PasswordChanged += OnPasswordChanged;
        AssociatedObject.Loaded += (s, e) => UpdatePlaceholderVisibility();
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        AssociatedObject.PasswordChanged -= OnPasswordChanged;
        base.OnDetaching();
    }

    /// <summary>
    /// Обрабатывает ввод строки пароля
    /// </summary>
    /// <param name="sender">
    /// Инициатор события
    /// </param>
    /// <param name="e">
    /// Аргументы события
    /// </param>
    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        AssociatedObject.PasswordChanged -= OnPasswordChanged;
        Password = AssociatedObject.Password;
        AssociatedObject.PasswordChanged += OnPasswordChanged;

        UpdatePlaceholderVisibility();
    }

    /// <summary>
    /// Обрабатывает сохранение пароля в свойство зависимостей
    /// </summary>
    /// <param name="obj">
    /// Свойство зависимостей
    /// </param>
    /// <param name="e">
    /// Аргументы события
    /// </param>
    private static void OnPasswordPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is PasswordBehavior behavior && behavior.AssociatedObject != null)
        {
            if (behavior.AssociatedObject.Password != (string)e.NewValue)
            {
                behavior.AssociatedObject.PasswordChanged -= behavior.OnPasswordChanged;
                behavior.AssociatedObject.Password = (string)e.NewValue ?? string.Empty;
                behavior.AssociatedObject.PasswordChanged += behavior.OnPasswordChanged;                
            }
        }
    }

    /// <summary>
    /// Обновляет отображение плейсхолдера в поле ввода
    /// </summary>
    private void UpdatePlaceholderVisibility()
    {
        if (_placeholder == null && AssociatedObject.Template != null)
        {
            _placeholder = AssociatedObject.Template.FindName("PlaceholderText", AssociatedObject) as TextBlock;
        }

        if (_placeholder != null)
        {
            // Если пароль пустой — показываем подсказку, если вбит хотя бы символ — скрываем!
            _placeholder.Visibility = string.IsNullOrEmpty(AssociatedObject.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
