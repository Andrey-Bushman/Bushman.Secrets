
namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Секрет.
    /// </summary>
    public interface ISecret {
        /// <summary>
        /// Строка, подлежащая шифрованию или расшифровке в составе секрета.
        /// </summary>
        string Data { get; }
        /// <summary>
        /// True - значение свойства Data находится в зашифрованном состоянии. False - в расшифрованном.
        /// </summary>
        bool IsEncrypted { get; }
        /// <summary>
        /// Настройки секрета.
        /// </summary>
        ISecretOptions Options { get; }
    }
}