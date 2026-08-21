using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Clio.Services
{
    /// <summary>
    /// Сервис для шифрования данных посредством алгоритма AES-256
    /// </summary>
    public static class CryptoService
    {
        /// <summary>
        /// Длина ключа шифрования в битах
        /// </summary>
        private const int KEY_BIT_SIZE = 256;

        /// <summary>
        /// Длина блока в битах
        /// </summary>
        private const int BLOCK_BIT_SIZE = 128;

        /// <summary>
        /// Количество проходов
        /// </summary>
        private const int ITERATION_COUNT = 100000;

        /// <summary>
        /// Зашифровывает данные по алгоритму AES-256 
        /// c применением дополнительного хэша (соли), 
        /// используя мастер-пароль в качестве ключа шифрования.
        /// </summary>
        /// <param name="plainText">
        /// Строка, подготовленная для шифрования.
        /// </param>
        /// <param name="password">
        /// Мастер-пароль.
        /// </param>
        /// <param name="hint">
        /// Подсказка для мастер-пароля (опционально).
        /// </param>
        /// <returns>
        /// Набор байт, содержащих зашифрованные данные.
        /// </returns>
        public static byte[] Encrypt(string plainText, string password, string hint)
        {
            if (string.IsNullOrEmpty(plainText)) return Array.Empty<byte>();

            var salt = RandomNumberGenerator.GetBytes(16);
            var iv = RandomNumberGenerator.GetBytes(16);

            // Готовим зашифрованные байты JSON контента
            byte[] encryptedJsonBytes;

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, ITERATION_COUNT, HashAlgorithmName.SHA256))
            {
                var key = deriveBytes.GetBytes(KEY_BIT_SIZE / 8);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KEY_BIT_SIZE;
                    aes.BlockSize = BLOCK_BIT_SIZE;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(key, iv))
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                        {
                            swEncrypt.Write(plainText);
                        }
                        // Сначала закрываем все обертки потоков, чтобы AES полностью финализировал 
                        // и записал блоки Padding в msEncrypt, и только ПОТОМ забираем готовый массив!
                        encryptedJsonBytes = msEncrypt.ToArray();
                    }
                }
            }

            // Готовим байты подсказки Base64
            var hintBase64 = !string.IsNullOrWhiteSpace(hint)
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(hint))
                : string.Empty;

            var hintBytes = Encoding.UTF8.GetBytes(hintBase64);

            // СОБИРАЕМ ИТОГОВЫЙ МАССИВ ФАЙЛА ЖЕСТКИМ КОПИРОВАНИЕМ БАЙТ:
            // Структура: [4 байта: длина подсказки] + [N байт: подсказка] + [16 байт: соль] + [16 байт: IV] + [AES данные]
            var finalFileBytes = new byte[4 + hintBytes.Length + 16 + 16 + encryptedJsonBytes.Length];

            Buffer.BlockCopy(BitConverter.GetBytes(hintBytes.Length), 0, finalFileBytes, 0, 4);

            if (hintBytes.Length > 0)
            {
                Buffer.BlockCopy(hintBytes, 0, finalFileBytes, 4, hintBytes.Length);
            }

            var saltOffset = 4 + hintBytes.Length;

            Buffer.BlockCopy(salt, 0, finalFileBytes, saltOffset, 16);
            Buffer.BlockCopy(iv, 0, finalFileBytes, saltOffset + 16, 16);

            var dataOffset = saltOffset + 32;
            Buffer.BlockCopy(encryptedJsonBytes, 0, finalFileBytes, dataOffset, encryptedJsonBytes.Length);

            return finalFileBytes;
        }

        /// <summary>
        /// Расшифровывает данные по алгоритму AES-256,
        /// используя мастер-пароль в качестве ключа шифрования.
        /// </summary>
        /// <param name="cipherData">
        /// Зашифрованные данные.
        /// </param>
        /// <param name="password">
        /// Мастер-пароль.
        /// </param>
        /// <returns>
        /// Расшифрованные данные в строковом представлении.
        /// </returns>
        /// <exception cref="CryptographicException">
        /// Ошибка при дешифровании данных.
        /// </exception>
        public static string Decrypt(byte[] cipherData, string password)
        {
            if (cipherData == null || cipherData.Length < 36)
            {
                throw new CryptographicException("Файл базы данных поврежден или пуст.");
            }

            var hintLength = BitConverter.ToInt32(cipherData, 0);
            var saltOffset = 4 + hintLength;

            if (cipherData.Length < saltOffset + 32)
            {
                throw new CryptographicException("Неверная или нарушенная структура файла.");
            }

            var salt = new byte[16];
            var iv = new byte[16];

            Buffer.BlockCopy(cipherData, saltOffset, salt, 0, 16);
            Buffer.BlockCopy(cipherData, saltOffset + 16, iv, 0, 16);

            var cryptoOffset = saltOffset + 32;
            var cipherTextLength = cipherData.Length - cryptoOffset;
            var encryptedJsonBytes = new byte[cipherTextLength];

            Buffer.BlockCopy(cipherData, cryptoOffset, encryptedJsonBytes, 0, cipherTextLength);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, ITERATION_COUNT, HashAlgorithmName.SHA256))
            {
                var key = deriveBytes.GetBytes(KEY_BIT_SIZE / 8);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KEY_BIT_SIZE;
                    aes.BlockSize = BLOCK_BIT_SIZE;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(key, iv))
                    using (var msDecrypt = new MemoryStream(encryptedJsonBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Читает подсказку для мастер-пароля, зашифрованную в Base64, игнорируя все остальное.
        /// </summary>
        /// <param name="cipherData">
        /// Содержимое файла в виде набора байт.
        /// </param>
        /// <returns>
        /// Строка, содержащая подсказку для мастер-пароля.
        /// </returns>
        public static string ReadBase64Hint(byte[] cipherData)
        {
            if (cipherData == null || cipherData.Length < 4)
            {
                return String.Empty;
            }

            var hintLength = BitConverter.ToInt32(cipherData, 0);

            if (hintLength == 0 || cipherData.Length < 4 + hintLength)
            {
                return String.Empty;
            }

            try
            {
                var hintBase64 = Encoding.UTF8.GetString(cipherData, 4, hintLength);
                var decodedBytes = Convert.FromBase64String(hintBase64);

                return Encoding.UTF8.GetString(decodedBytes);
            }
            catch
            {
                return "Не удалось прочитать подсказку.";
            }
        }
    }
}
