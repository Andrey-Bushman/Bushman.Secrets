using Bushman.Secrets.Abstractions.Models;
using System;

namespace Bushman.Secrets.Services {
    /// <summary>
    /// Интерфейс для валидации, парсинга, шифрования и расшифровки секретов.
    /// </summary>
    public interface IEncryptor {
        /// <summary>
        /// Базовые настройки для работы с секретами.
        /// </summary>
        ISecretOptionsBase OptionsBase { get; }
        /// <summary>
        /// Расшифровать секрет.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий расшифровке.</param>
        /// <returns>Если секрет уже расшифрован, то возвращается объект, полученный в параметре secret. 
        /// В противном случае выполняется расшифровка и возвращается новый экземпляр ISecret.</returns>
        ISecret Decrypt(ISecret secret);
        /// <summary>
        /// Расшифровать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно расшифровать все секреты.</param>
        /// <returns>Возвращается строка с расшифрованными секретами.</returns>
        string Decrypt(string text);
        /// <summary>
        /// Зашифровать секрет.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий шифрованию.</param>
        /// <returns>Если секрет уже зашифрован, то возвращается объект, полученный в параметре secret. 
        /// В противном случае выполняется шифрование и возвращается новый экземпляр ISecret.</returns>
        ISecret Encrypt(ISecret secret);
        /// <summary>
        /// Зашифровать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно зашифровать все секреты.</param>
        /// <returns>Возвращается строка с зашифрованными секретами.</returns>
        string Encrypt(string text);
        /// <summary>
        /// Получить распакованное значение секрета.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий распаковке.</param>
        /// <returns>Распакованное значение секрета.</returns>
        string Expand(ISecret secret);
        /// <summary>
        /// Распаковать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно распаковать все секреты.</param>
        /// <returns>Возвращается строка с распакованными секретами.</returns>
        string Expand(string text);
        /// <summary>
        /// Получить количество расшифрованных секретов в строке.
        /// </summary>
        /// <param name="content">Строка, для которой следует выполнить подсчёт.</param>
        /// <returns>Количество расшифрованных секретов в строке.</returns>
        int GetDecryptedSecretsCount(string content);
        /// <summary>
        /// Получить количество зашифрованных секретов в строке.
        /// </summary>
        /// <param name="content">Строка, для которой следует выполнить подсчёт.</param>
        /// <returns>Количество зашифрованных секретов в строке.</returns>
        int GetEncryptedSecretsCount(string content);
        /// <summary>
        /// Выяснить, является ли полученная строка расшифрованным секретом.
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является расшифрованным секретом. False - не является.</returns>
        bool IsDecryptedSecret(string value);
        /// <summary>
        /// Выяснить, является ли полученная строка зашифрованным секретом.
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является зашифрованным секретом. False - не является.</returns>
        bool IsEncryptedSecret(string value);
        /// <summary>
        /// Выяснить, является ли полученная строка секретом (не важно, расшифрованным или зашифрованным).
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является секретом. False - не является.</returns>
        bool IsSecret(string value);
        /// <summary>
        /// Распарсить строку в экземпляр ISecret.
        /// </summary>
        /// <param name="secret">Строка, подлежащая парсингу.</param>
        /// <returns>Экземпляр ISecret.</returns>
        ISecret ParseSecret(string secret);
    }
}