using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Clio.Helpers;

/// <summary>
/// Наблюдаемый объект MVVM
/// </summary>
public class ObservableObject : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Устанавливает новое значение, оповещая при этом подписчиков.
    /// </summary>
    /// <typeparam name="T">
    /// Тип значения.
    /// </typeparam>
    /// <param name="storage">
    /// Свойство, в которое сохраняется новое значение.
    /// </param>
    /// <param name="value">
    /// Новое значение.
    /// </param>
    /// <param name="propertyName">
    /// Имя обновляемого свойства
    /// </param>
    /// <returns>
    /// Результат обновления свойства. 
    /// Вернет ложный результат, если старое и новое значения идентичны.
    /// </returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}