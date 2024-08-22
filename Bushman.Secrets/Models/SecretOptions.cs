using Bushman.Secrets.Abstractions.Models;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Настройки секрета.
    /// </summary>
    public sealed class SecretOptions : ISecretOptions {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки.</param>
        /// <param name="storeLocation">Хранилище сертификата.</param>
        /// <param name="hashAlgorithmName">Наименование алгоритма шифрования.</param>
        /// <param name="thumbprint">Отпечаток сертификата.</param>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public SecretOptions(
            ISecretOptionsBase optionsBase,
            StoreLocation storeLocation,
            HashAlgorithmName hashAlgorithmName,
            string thumbprint) {

            if (optionsBase == null) throw new ArgumentNullException(nameof(optionsBase));
            if (string.IsNullOrWhiteSpace(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));

            OptionsBase = optionsBase;
            StoreLocation = storeLocation;
            HashAlgorithmName = hashAlgorithmName;
            Thumbprint = thumbprint;
        }
        /// <summary>
        /// Базовые настройки.
        /// </summary>
        public ISecretOptionsBase OptionsBase { get; }
        /// <summary>
        /// Хранилище сертификата.
        /// </summary>
        public StoreLocation StoreLocation { get; }
        /// <summary>
        /// Наименование алгоритма шифрования.
        /// </summary>
        public HashAlgorithmName HashAlgorithmName { get; }
        /// <summary>
        /// Отпечаток сертификата.
        /// </summary>
        public string Thumbprint { get; }

        /// <summary>
        /// Переопределённое строковое представление объекта.
        /// </summary>
        /// <returns>Строковое представление объекта.</returns>
        public override string ToString() {
            return string.Join($"{OptionsBase.FieldSeparator}", StoreLocation, HashAlgorithmName, Thumbprint);
        }
    }
}
