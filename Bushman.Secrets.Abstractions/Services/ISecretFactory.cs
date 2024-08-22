using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Services;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bushman.Secrets.Abstractions.Services {
    /// <summary>
    /// Фабрика для создания экземпляров интерфейсов, необходимых для работы с секретами.
    /// </summary>
    public interface ISecretFactory {
        ITagPair CreateTagPair(string openTag, string closeTag);
        /// <summary>
        /// Создать базовые настройки секретов.
        /// </summary>
        /// <returns>Экземпляр ISecretOptionsBase.</returns>
        ISecretOptionsBase CreateSecretOptionsBase();
        /// <summary>
        /// Создать базовые настройки секретов.
        /// </summary>
        /// <param name="encoding">Кодировка содержимого секрета.</param>
        /// <param name="fieldSeparator">Разделитель полей секрета.</param>
        /// <param name="encryptedTagPair">Пара тегов для зашифрованного секрета.</param>
        /// <param name="decryptedTagPair">Пара тегов для расшифрованного секрета.</param>
        /// <returns>Экземпляр ISecretOptionsBase.</returns>
        ISecretOptionsBase CreateSecretOptionsBase(Encoding encoding, char fieldSeparator, ITagPair encryptedTagPair, ITagPair decryptedTagPair);
        /// <summary>
        /// Создать настройки секрета.
        /// </summary>
        /// <param name="storeLocation">Хранилище секрета.</param>
        /// <param name="hashAlgorithmName">Наименование алгоритма шифрования.</param>
        /// <param name="thumbprint">Отпечаток сертификата.</param>
        /// <returns>Экземпляр ISecretOptions.</returns>
        ISecretOptions CreateSecretOptions(StoreLocation storeLocation, HashAlgorithmName hashAlgorithmName, string thumbprint);
        /// <summary>
        /// Создать настройки секрета.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки секрета.</param>
        /// <param name="storeLocation">Хранилище секрета.</param>
        /// <param name="hashAlgorithmName">Наименование алгоритма шифрования.</param>
        /// <param name="thumbprint">Отпечаток сертификата.</param>
        /// <returns>Экземпляр ISecretOptions.</returns>
        ISecretOptions CreateSecretOptions(ISecretOptionsBase optionsBase, StoreLocation storeLocation, HashAlgorithmName hashAlgorithmName, string thumbprint);
        /// <summary>
        /// Создать экземпляр секрета.
        /// </summary>
        /// <param name="options">Настройки секрета.</param>
        /// <param name="data">Содержимое секрета.</param>
        /// <param name="isEncrypted">True - содержимое секрета зашифровано. False - расшифровано.</param>
        /// <returns>Экземпляр ISecret.</returns>
        ISecret CreateSecret(ISecretOptions options, string data, bool isEncrypted);
        /// <summary>
        /// Создать экземпляр IEncryptor для парсинга, валидации, шифрования, расшифровки и распаковки секретов.
        /// </summary>
        /// <returns>Экземпляр IEncryptor.</returns>
        IEncryptor CreateEncryptor();
        /// <summary>
        /// Создать экземпляр IEncryptor для парсинга, валидации, шифрования, расшифровки и распаковки секретов.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки секретов.</param>
        /// <returns>Экземпляр IEncryptor.</returns>
        IEncryptor CreateEncryptor(ISecretOptionsBase optionsBase);
    }
}
