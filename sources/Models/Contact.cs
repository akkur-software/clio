using Clio.Helpers;
using System.Text;

namespace Clio.Models;

/// <summary>
/// Контакт
/// </summary>
public class Contact : ObservableObject
{
    private string _firstName;

    private string _lastName;

    private string _middleName;

    private string _description;

    private string _phone;

    private string _phoneAdvanced;

    private string _email;

    private string _telegramId;

    private string _vkId;

    private DateOnly? _birthDate;

    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName 
    {
        get => _firstName; 
        set
        {
            if (SetProperty(ref _firstName, value))
            {
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName 
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    /// <summary>
    /// Отчество
    /// </summary>
    public string MiddleName
    {
        get => _middleName;
        set => SetProperty(ref _middleName, value);
    }

    /// <summary>
    /// Описание контакта
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Основной номер телефона
    /// </summary>
    public string Phone 
    {
        get => _phone; 
        set => SetProperty(ref _phone, value);
    }

    /// <summary>
    /// Дополнительный номер телефона
    /// </summary>
    public string PhoneAdvanced
    {
        get => _phoneAdvanced;
        set => SetProperty(ref _phoneAdvanced, value);
    }    

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string Email 
    {
        get => _email; 
        set => SetProperty(ref _email, value); 
    }

    /// <summary>
    /// Идентификатор аккаунта в Telegram
    /// </summary>
    public string TelegramId
    {
        get => _telegramId;
        set => SetProperty(ref _telegramId, value);
    }

    /// <summary>
    /// Идентификатор аккаунта в Vk
    /// </summary>
    public string VkId
    {
        get => _vkId;
        set => SetProperty(ref _vkId, value);
    }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateOnly? BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => (String.IsNullOrEmpty(FirstName) && String.IsNullOrEmpty(LastName) && String.IsNullOrEmpty(MiddleName)) 
        ? "Анонимный контакт" 
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
