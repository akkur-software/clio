using Clio.Helpers;
using Clio.Services;
using Clio.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Clio.ViewModels;

/// <summary>
/// Класс-заглушка для диалога удаления контакта
/// </summary>
public class DeleteDialogViewModel { }

/// <summary>
/// Класс-заглушка для диалога авторизации
/// </summary>
public class PasswordDialogViewModel { }

/// <summary>
/// Главная модель представления приложения.
/// </summary>
public class MainViewModel : ObservableObject
{    
    private ObservableCollection<Contact> _contacts;
    private object _activeDialog;
    private ICollectionView _contactsView;
    private Contact _selectedContact;
    private string _searchText;
    private bool _isReadOnly = true;
    private string _editSaveButtonText = "Редактировать";
    private string _masterPassword;
    private bool _isNewDatabase;
    private string _passwordHint;

    /// <summary>
    /// Команда добавления контакта.
    /// </summary>
    public ICommand AddContactCommand { get; }

    /// <summary>
    /// Команда удаления контакта.
    /// </summary>
    public ICommand DeleteContactCommand { get; }

    /// <summary>
    /// Команда сохранения/редактирования контакта.
    /// </summary>
    public ICommand ToggleEditSaveCommand { get; }

    /// <summary>
    /// Команда подтверждения удаления контакта.
    /// </summary>
    public ICommand ConfirmDeleteCommand { get; }

    /// <summary>
    /// Команда авторизации и разблокировки базы данных.
    /// </summary>
    public ICommand UnlockDatabaseCommand { get; }

    /// <summary>
    /// Представление контактов.
    /// </summary>
    public ICollectionView ContactsView => _contactsView;

    /// <summary>
    /// Выбранный пользователем контакт.
    /// </summary>
    public Contact SelectedContact
    {
        get => _selectedContact;
        set
        {
            if (SetProperty(ref _selectedContact, value))
            {
                // Если пользователь переключил контакт, принудительно возвращаем режим "Только чтение"
                IsReadOnly = true;
                EditSaveButtonText = "Редактировать";
            }
        }
    }

    /// <summary>
    /// Активное модальное диалоговое окно.
    /// </summary>
    public object ActiveDialog
    {
        get => _activeDialog;
        set => SetProperty(ref _activeDialog, value);
    }

    /// <summary>
    /// Текст строки поиска.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _contactsView.Refresh();
            }
        }
    }

    /// <summary>
    /// Находится ли карточка контакта в режиме "только чтение".
    /// </summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    /// <summary>
    /// Текст кнопки изменения данных в карточке контакта.
    /// </summary>
    public string EditSaveButtonText
    {
        get => _editSaveButtonText;
        set => SetProperty(ref _editSaveButtonText, value);
    }

    /// <summary>
    /// Мастер-пароль пользователя.
    /// </summary>
    public string MasterPassword
    {
        get => _masterPassword;
        set => SetProperty(ref _masterPassword, value);
    }

    /// <summary>
    /// Является ли база данных только что созданным файлом.
    /// </summary>
    public bool IsNewDatabase
    {
        get => _isNewDatabase;
        set => SetProperty(ref _isNewDatabase, value);
    }

    /// <summary>
    /// Подсказка для мастер-пароля.
    /// </summary>
    public string PasswordHint
    {
        get => _passwordHint;
        set => SetProperty(ref _passwordHint, value);
    }

    /// <summary>
    /// Предоставляет новый экземпляр <see cref="MainViewModel"/>.
    /// </summary>
    public MainViewModel()
    {
        _contacts = new ObservableCollection<Contact>();

        // 2. Создаем специальную "обертку-представление" поверх коллекции для живого поиска
        _contactsView = CollectionViewSource.GetDefaultView(_contacts);
        _contactsView.Filter = FilterContacts;

        // 3. Инициализируем команды
        AddContactCommand = new RelayCommand(OnAddContact);
        DeleteContactCommand = new RelayCommand(OnDeleteContact, CanDeleteContact);
        ToggleEditSaveCommand = new RelayCommand(OnToggleEditSave, CanToggleEditSave);
        ConfirmDeleteCommand = new RelayCommand(OnConfirmDelete);
        UnlockDatabaseCommand = new RelayCommand(OnUnlockDatabase, CanUnlockDatabase);

        IsNewDatabase = !StorageService.IsExists();

        PasswordHint = !IsNewDatabase 
            ? StorageService.LoadPasswordHint() 
            : String.Empty;

        // Включаем матовый оверлей ввода пароля прямо при старте приложения
        ActiveDialog = new PasswordDialogViewModel();

        _contactsView.SortDescriptions.Clear();

        var sortLastNameDesc = new SortDescription("FullName", ListSortDirection.Ascending);
        _contactsView.SortDescriptions.Add(sortLastNameDesc);
    }    

    /// <summary>
    /// Осуществляет фильтрацию контактов, 
    /// согласно поисковому запросу.
    /// </summary>
    /// <param name="obj">
    /// Текущий контакт из списка.
    /// </param>
    /// <returns>
    /// Возвращает статус совпадения данных контакта 
    /// с введенным поисковым текстовым запросом.
    /// </returns>
    private bool FilterContacts(object obj)
    {
        // Если строка поиска пустая, пускаем элемент в список (отображаем всех)
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        if (obj is Contact contact)
        {
            var searchQuery = SearchText.Trim().ToLower();

            var contactProperties = new List<string>
            {
                contact.FullName,
                contact.Description,
                contact.Phone,
                contact.PhoneAdvanced,
                contact.Email,
                contact.TelegramId,
                contact.VkId
            };

            var isMatched = contactProperties.Any(prop => prop != null && prop.ToLower().Contains(searchQuery));

            // Если хоть один критерий совпал — контакт остается на экране
            return isMatched;
        }
        return false;
    }

    /// <summary>
    /// Обрабатывает добавление нового контакта в список.
    /// </summary>
    private void OnAddContact(object parameter)
    {
        var newContact = new Contact 
        { 
            FirstName = String.Empty,
            LastName = String.Empty,
            MiddleName = String.Empty,
            Description = String.Empty,
            Phone = String.Empty,
            PhoneAdvanced = String.Empty,
            Email = String.Empty,
            TelegramId = String.Empty,
            VkId = String.Empty,
            BirthDate = null
        };

        _contacts.Add(newContact);

        // Автоматически выбираем его.
        SelectedContact = newContact;

        // Сразу включаем режим редактирования для нового контакта.
        IsReadOnly = false;
        EditSaveButtonText = "Сохранить изменения";

        _contactsView.Refresh();
    }

    /// <summary>
    /// Обрабатывает удаление контакта из списка.
    /// </summary>
    private void OnDeleteContact(object parameter)
    {
        if (SelectedContact != null)
        {
            ActiveDialog = new DeleteDialogViewModel();
        }
    }

    /// <summary>
    /// Обрабатывает подтверждение или отказ от удаления контакта.
    /// </summary>
    private void OnConfirmDelete(object parameter)
    {
        if (parameter?.ToString() == "Yes" && SelectedContact != null)
        {
            _contacts.Remove(SelectedContact);
            SelectedContact = null;

            StorageService.Save(_contacts, MasterPassword, PasswordHint);
        }

        // Закрываем диалог
        ActiveDialog = null;
        _contactsView.Refresh();
    }
    
    /// <summary>
    /// Обрабатывает переключение режимов редактирования и чтения 
    /// в карточке контакта.
    /// </summary>
    private void OnToggleEditSave(object parameter)
    {
        if (IsReadOnly)
        {
            IsReadOnly = false;
            EditSaveButtonText = "Сохранить изменения";
        }
        else
        {
            System.Windows.Input.Keyboard.ClearFocus();

            try
            {
                StorageService.Save(_contacts, MasterPassword, PasswordHint);
                _contactsView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения зашифрованного файла: {ex.Message}", "Ошибка Clio", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsReadOnly = true;
            EditSaveButtonText = "Редактировать";
        }
    }
    
    /// <summary>
    /// Обрабатывает расшифровку базы данных.
    /// </summary>
    private void OnUnlockDatabase(object parameter)
    {
        if (string.IsNullOrEmpty(MasterPassword))
        {
            return;
        }

        try
        {
            if (IsNewDatabase)
            {
                // Если это первый запуск — просто создаем пустую базу под этот пароль
                _contacts.Clear();
                StorageService.Save(_contacts, MasterPassword, PasswordHint);
            }
            else
            {
                // Если архив уже существует — пытаемся его расшифровать
                var loadedContacts = StorageService.Load(MasterPassword);
                _contacts.Clear();

                foreach (var contact in loadedContacts)
                {
                    _contacts.Add(contact);
                }
            }

            // Дешифрование прошло успешно! Закрываем оверлей пароля и пускаем юзера в программу
            ActiveDialog = null;
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            // Если пароль не подошел — сработает защита AES
            MessageBox.Show($"Введен неверный мастер-пароль! {ex.Message}", "Ошибка дешифрования", MessageBoxButton.OK, MessageBoxImage.Stop);
            MasterPassword = string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Критическая ошибка хранилища: {ex.Message}", "Ошибка Clio", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    /// <summary>
    /// Проверяет, можно ли удалить контакт в текущий момент времени.
    /// </summary>
    /// <returns>
    /// Возвращает <see langword="true"/>, если удаление разрешено.
    /// </returns>
    private bool CanDeleteContact(object parameter) => SelectedContact != null;

    /// <summary>
    /// Проверяет, можно ли переключить 
    /// режимы чтения и редактирования контакта 
    /// в текущий момент времени.
    /// </summary>
    /// <returns>
    /// Возвращает <see langword="true"/>, если переключение разрешено.
    /// </returns>
    private bool CanToggleEditSave(object p) => SelectedContact != null;

    /// <summary>
    /// Проверяет, можно ли расшифровать базу в текущий момент времени.
    /// </summary>
    /// <returns>
    /// Возвращает <see langword="true"/>, если расшифровка разрешена.
    /// </returns>
    private bool CanUnlockDatabase(object parameter)
    {
        return !string.IsNullOrEmpty(MasterPassword) || IsNewDatabase;
    }
}
