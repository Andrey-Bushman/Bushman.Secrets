﻿using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Models;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bushman.Secrets.Services {
    /// <summary>
    /// Фабрика для создания экземпляров интерфейсов, необходимых для работы с секретами.
    /// </summary>
    public sealed class SecretFactory : ISecretFactory {
        /// <summary>
        /// Создать экземпляр незашифрованного секрета.
        /// </summary>
        /// <param name="options">Настройки секрета.</param>
        /// <param name="data">Содержимое секрета.</param>
        /// <returns>Экземпляр ISecret.</returns>
        public ISecret CreateDecryptedSecret(ISecretOptions options, string data) => new Secret(options, data, false);
        /// <summary>
        /// Создать настройки секрета.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки секрета.</param>
        /// <param name="storeLocation">Хранилище секрета.</param>
        /// <param name="thumbprint">Отпечаток сертификата.</param>
        /// <returns>Экземпляр ISecretOptions.</returns>
        public ISecretOptions CreateSecretOptions(ISecretOptionsBase optionsBase, StoreLocation storeLocation, string thumbprint) =>
            new SecretOptions(optionsBase, storeLocation, thumbprint);
        /// <summary>
        /// Создать настройки секрета.
        /// </summary>
        /// <param name="storeLocation">Хранилище секрета.</param>
        /// <param name="thumbprint">Отпечаток сертификата.</param>
        /// <returns>Экземпляр ISecretOptions.</returns>
        public ISecretOptions CreateSecretOptions(StoreLocation storeLocation, string thumbprint) =>
            new SecretOptions(CreateSecretOptionsBase(), storeLocation, thumbprint);
        /// <summary>
        /// Создать базовые настройки секретов.
        /// </summary>
        /// <returns>Экземпляр ISecretOptionsBase.</returns>
        public ISecretOptionsBase CreateSecretOptionsBase() => new SecretOptionsBase();
        /// <summary>
        /// Создать базовые настройки секретов.
        /// </summary>
        /// <param name="encoding">Кодировка содержимого секрета.</param>
        /// <param name="fieldSeparator">Разделитель полей секрета.</param>
        /// <param name="encryptedTagPair">Пара тегов для зашифрованного секрета.</param>
        /// <param name="decryptedTagPair">Пара тегов для расшифрованного секрета.</param>
        /// <param name="aesKeySize">Размер секретного ключа в битах для симметричного алгоритма шифрования.</param>
        /// <param name="aesCipherMode">Режим операции симметричного алгоритма.</param>
        /// <returns>Экземпляр ISecretOptionsBase.</returns>
        public ISecretOptionsBase CreateSecretOptionsBase(Encoding encoding, char fieldSeparator, ITagPair encryptedTagPair,
            ITagPair decryptedTagPair, int aesKeySize, CipherMode aesCipherMode) =>
            new SecretOptionsBase(encoding, fieldSeparator, encryptedTagPair, decryptedTagPair, aesKeySize, aesCipherMode);
        /// <summary>
        /// Создать пару тегов секрета.
        /// </summary>
        /// <param name="openTag">Открывающий тег секрета.</param>
        /// <param name="closeTag">Закрывающий тег секрета.</param>
        /// <returns>Экземпляр ITagPair.</returns>
        public ITagPair CreateTagPair(string openTag, string closeTag) => new TagPair(openTag, closeTag);
        /// <summary>
        /// Создать экземпляр IEncryptor для парсинга, валидации, шифрования, расшифровки и распаковки секретов.
        /// </summary>
        /// <returns>Экземпляр IEncryptor.</returns>
        public IEncryptor CreateEncryptor() => new Encryptor();
        /// <summary>
        /// Создать экземпляр IEncryptor для парсинга, валидации, шифрования, расшифровки и распаковки секретов.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки секретов.</param>
        /// <returns>Экземпляр IEncryptor.</returns>
        public IEncryptor CreateEncryptor(ISecretOptionsBase optionsBase) => new Encryptor(optionsBase);
    }
}
