using Clio.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clio.Services;

/// <summary>
/// Сервис для загрузки и выгрузки данных относительно файла базы данных.
/// </summary>
public static class StorageService
{
    /// <summary>
    /// Имя базы данных на жестком диске
    /// </summary>
    private const string DATABASE_FILENAME = "db.clio";

    /// <summary>
    /// Проверяет наличие файла базы данных на диске.
    /// </summary>
    /// <returns>
    /// Возвращает <see langword="true"/>, если файл существует, 
    /// иначе возвращает <see langword="false"/>.
    /// </returns>
    public static bool IsExists() => File.Exists(DATABASE_FILENAME);

    /// <summary>
    /// Сохраняет коллекцию контактов в файл базы данных, 
    /// с использованием мастер-пароля и подсказки к нему.
    /// </summary>
    /// <param name="contacts">
    /// Коллекция контактов <see cref="Contact"/>.
    /// </param>
    /// <param name="password">
    /// Мастер-пароль.
    /// </param>
    /// <param name="hint">
    /// Подсказка для мастер-пароля.
    /// </param>
    public static void Save(IEnumerable<Contact> contacts, string password, string hint)
    {
        // Теперь, даже если XAML-маски или фокус будут дёргать свойства контактов в UI,
        // наш сериализатор будет спокойно работать со статичными данными в памяти!
        var contactsSnapshot = contacts.ToArray();

        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var jsonString = JsonSerializer.Serialize(contactsSnapshot, options);
        var encryptedData = CryptoService.Encrypt(jsonString, password, hint);

        File.WriteAllBytes(DATABASE_FILENAME, encryptedData);
    }

    /// <summary>
    /// Загружает существующую подсказку 
    /// для мастер-пароля из базы данных.
    /// </summary>
    /// <returns>
    /// Строка, содержащая подсказку для мастер-пароля.
    /// </returns>
    public static string LoadPasswordHint()
    {
        if (!IsExists())
        {
            return string.Empty;
        }

        var encryptedData = File.ReadAllBytes(DATABASE_FILENAME);
        return CryptoService.ReadBase64Hint(encryptedData);
    }

    /// <summary>
    /// Загружает коллекцию контактов из файла базы данных.
    /// </summary>
    /// <param name="password">
    /// Мастер-пароль.
    /// </param>
    /// <returns>
    /// Новая коллекция контактов <see cref="Contact"/>.
    /// </returns>
    public static IEnumerable<Contact> Load(string password)
    {
        if (!IsExists())
        {
            return new List<Contact>();
        }

        var encryptedData = File.ReadAllBytes(DATABASE_FILENAME);
        var jsonString = CryptoService.Decrypt(encryptedData, password);

        if (String.IsNullOrEmpty(jsonString))
        {
            return new List<Contact>();
        }

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Deserialize<List<Contact>>(jsonString, options);
    }
}
