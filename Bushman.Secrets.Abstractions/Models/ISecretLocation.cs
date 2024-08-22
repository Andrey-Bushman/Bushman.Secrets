
namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Информация о размещении секрета в тексте.
    /// </summary>
    public interface ISecretLocation {
        /// <summary>
        /// Индекс, с которого начинается секрет в тексте.
        /// </summary>
        int StartIndex { get; }
        /// <summary>
        /// Индекс, следующий за окончанием секрета в тексте.
        /// </summary>
        int EndIndex { get; }
        /// <summary>
        /// Секрет, размещённый в тексте.
        /// </summary>
        ISecret Secret { get; }
    }
}