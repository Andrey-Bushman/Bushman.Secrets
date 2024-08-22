using Bushman.Secrets.Abstractions.Models;
using System;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Секрет.
    /// </summary>
    public sealed class Secret : ISecret {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="options">Настройки секрета.</param>
        /// <param name="data">Строка, подлежащая шифрованию или расшифровке в составе секрета.</param>
        /// <param name="isEncrypted">True - значение свойства Data находится в зашифрованном состоянии.
        /// False - в расшифрованном.</param>
        /// <exception cref="ArgumentNullException">Вместо настроек параметру options передан null.</exception>
        public Secret(ISecretOptions options, string data, bool isEncrypted) {

            if (options == null) throw new ArgumentNullException(nameof(options));

            Options = options;
            Data = data;
            IsEncrypted = isEncrypted;
        }
        /// <summary>
        /// Настройки секрета.
        /// </summary>
        public ISecretOptions Options { get; }
        /// <summary>
        /// Строка, подлежащая шифрованию или расшифровке в составе секрета.
        /// </summary>
        public string Data { get; }
        /// <summary>
        /// True - значение свойства Data находится в зашифрованном состоянии. False - в расшифрованном.
        /// </summary>
        public bool IsEncrypted { get; }

        /// <summary>
        /// Переопределённое строковое представление объекта.
        /// </summary>
        /// <returns>Строковое представление объекта.</returns>
        public override string ToString() {

            var prefix = IsEncrypted ? Options.OptionsBase.EncryptedTagPair.OpenTag : Options.OptionsBase.DecryptedTagPair.OpenTag;
            var suffix = IsEncrypted ? Options.OptionsBase.EncryptedTagPair.CloseTag : Options.OptionsBase.DecryptedTagPair.CloseTag;

            return string.Join($"{Options.OptionsBase.FieldSeparator}", prefix, Options, Data, suffix);
        }
    }
}
