using System.Windows.Input;

namespace Clio.Helpers;

/// <summary>
/// Команда MVVM, инициируемая пользователем
/// </summary>
public class RelayCommand : ICommand
{
    /// <summary>
    /// Метод-обработчик команды
    /// </summary>
    private readonly Action<object> _execute;

    /// <summary>
    /// Метод, проверяющий возможность выполнения команды.
    /// </summary>
    private readonly Func<object, bool> _canExecute;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="execute">
    /// Метод-обработчик команды
    /// </param>
    /// <param name="canExecute">
    /// Метод, проверяющий возможность выполнения команды.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Не задан метод-обработчик команды.
    /// </exception>
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

    /// <inheritdoc/>
    public void Execute(object parameter) => _execute(parameter);

    /// <inheritdoc/>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}