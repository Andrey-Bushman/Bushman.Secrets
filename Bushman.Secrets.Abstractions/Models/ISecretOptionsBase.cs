using System.Security.Cryptography;
using System.Text;

namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Базовые настройки для работы с секретами.
    /// </summary>
    public interface ISecretOptionsBase {
        /// <summary>
        /// Теги расшифрованного секрета.
        /// </summary>
        ITagPair DecryptedTagPair { get; }
        /// <summary>
        /// Кодировка текстового представления секрета.
        /// </summary>
        Encoding Encoding { get; }
        /// <summary>
        /// Теги зашифрованного секрета.
        /// </summary>
        ITagPair EncryptedTagPair { get; }
        /// <summary>
        /// Разделитель значений в строковом представлении секрета.
        /// </summary>
        char FieldSeparator { get; }

        /// <summary>
        /// Размер секретного ключа в битах для симметричного алгоритма шифрования.
        /// </summary>
        int AesKeySize { get; }

        /// <summary>
        /// Режим операции симметричного алгоритма.
        /// </summary>
        CipherMode AesCipherMode { get; }
    }
}