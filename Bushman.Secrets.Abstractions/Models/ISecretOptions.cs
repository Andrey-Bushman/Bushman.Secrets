using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Настройки секрета.
    /// </summary>
    public interface ISecretOptions {
        /// <summary>
        /// Базовые настройки.
        /// </summary>
        ISecretOptionsBase OptionsBase { get; }
        /// <summary>
        /// Наименование алгоритма шифрования.
        /// </summary>
        HashAlgorithmName HashAlgorithmName { get; }
        /// <summary>
        /// Хранилище сертификата.
        /// </summary>
        StoreLocation StoreLocation { get; }
        /// <summary>
        /// Отпечаток сертификата.
        /// </summary>
        string Thumbprint { get; }
    }
}