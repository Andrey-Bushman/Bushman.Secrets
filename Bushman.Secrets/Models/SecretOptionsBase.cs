using Bushman.Secrets.Abstractions.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Базовые настройки для работы с секретами.
    /// </summary>
    public sealed class SecretOptionsBase : ISecretOptionsBase {

        /// <summary>
        /// Значение по умолчанию для пары тегов зашифрованного секрета.
        /// </summary>
        public static readonly ITagPair DefaultEncryptedTagPair = new TagPair("%%ENCRYPTED", "ENCRYPTED%%");
        /// <summary>
        /// Значение по умолчанию для пары тегов расшифрованного секрета.
        /// </summary>
        public static readonly ITagPair DefaultDecryptedTagPair = new TagPair("%%DECRYPTED", "DECRYPTED%%");
        /// <summary>
        /// Значение по умолчанию для разделителя полей в секрете.
        /// </summary>
        public const char DefaultFieldSeparator = '|';
        /// <summary>
        /// Значение по умолчанию для кодировки, в которой записывается содержимое свойства Data в секрете.
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Размер секретного ключа в битах для симметричного алгоритма шифрования.
        /// </summary>
        public int AesKeySize { get; }

        /// <summary>
        /// Режим операции симметричного алгоритма.
        /// </summary>
        public CipherMode AesCipherMode { get; }

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="encoding">Кодировка текстового представления секрета.</param>
        /// <param name="fieldSeparator">Разделитель значений в строковом представлении секрета.</param>
        /// <param name="encryptedTagPair">Теги зашифрованного секрета.</param>
        /// <param name="decryptedTagPair">Теги расшифрованного секрета.</param>
        /// <param name="aesKeySize">Размер секретного ключа в битах для симметричного алгоритма шифрования.</param>
        /// <param name="aesCipherMode">Режим операции симметричного алгоритма.</param>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public SecretOptionsBase(Encoding encoding, char fieldSeparator, ITagPair encryptedTagPair, ITagPair decryptedTagPair, int aesKeySize, CipherMode aesCipherMode) {

            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            if (encryptedTagPair == null) throw new ArgumentNullException(nameof(encryptedTagPair));
            if (decryptedTagPair == null) throw new ArgumentNullException(nameof(decryptedTagPair));

            Encoding = encoding;
            FieldSeparator = fieldSeparator;
            EncryptedTagPair = encryptedTagPair;
            DecryptedTagPair = decryptedTagPair;
            AesKeySize = aesKeySize;
            AesCipherMode = aesCipherMode;
        }
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public SecretOptionsBase() {

            FieldSeparator = DefaultFieldSeparator;
            EncryptedTagPair = DefaultEncryptedTagPair;
            DecryptedTagPair = DefaultDecryptedTagPair;
            Encoding = DefaultEncoding;
            AesKeySize = 256;
            AesCipherMode = CipherMode.CBC;
        }
        /// <summary>
        /// Теги расшифрованного секрета.
        /// </summary>
        public ITagPair DecryptedTagPair { get; }
        /// <summary>
        /// Кодировка текстового представления секрета.
        /// </summary>
        public Encoding Encoding { get; }
        /// <summary>
        /// Теги зашифрованного секрета.
        /// </summary>
        public ITagPair EncryptedTagPair { get; }
        /// <summary>
        /// Разделитель значений в строковом представлении секрета.
        /// </summary>
        public char FieldSeparator { get; }
    }
}
