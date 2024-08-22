using Bushman.Secrets.Abstractions.Models;
using System;
using System.Text;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Базовые настройки для работы с секретами.
    /// </summary>
    public sealed class SecretOptionsBase : ISecretOptionsBase {

        public static readonly ITagPair DefaultEncryptedTagPair = new TagPair("%%ENCRYPTED", "ENCRYPTED%%");
        public static readonly ITagPair DefaultDecryptedTagPair = new TagPair("%%DECRYPTED", "DECRYPTED%%");
        public const char DefaultFieldSeparator = '|';
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="encoding">Кодировка текстового представления секрета.</param>
        /// <param name="fieldSeparator">Разделитель значений в строковом представлении секрета.</param>
        /// <param name="encryptedTagPair">Теги зашифрованного секрета.</param>
        /// <param name="decryptedTagPair">Теги расшифрованного секрета.</param>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public SecretOptionsBase(Encoding encoding, char fieldSeparator, ITagPair encryptedTagPair, ITagPair decryptedTagPair) {

            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            if (encryptedTagPair == null) throw new ArgumentNullException(nameof(encryptedTagPair));
            if (decryptedTagPair == null) throw new ArgumentNullException(nameof(decryptedTagPair));

            Encoding = encoding;
            FieldSeparator = fieldSeparator;
            EncryptedTagPair = encryptedTagPair;
            DecryptedTagPair = decryptedTagPair;
        }
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public SecretOptionsBase() {

            FieldSeparator = DefaultFieldSeparator;
            EncryptedTagPair = DefaultEncryptedTagPair;
            DecryptedTagPair = DefaultDecryptedTagPair;
            Encoding = DefaultEncoding;
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
