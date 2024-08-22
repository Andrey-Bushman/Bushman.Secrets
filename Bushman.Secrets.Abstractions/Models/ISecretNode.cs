
namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Узел секрета.
    /// </summary>
    public interface ISecretNode : INode {
        /// <summary>
        /// Информация о размещении секрета в тексте.
        /// </summary>
        ISecretLocation Value { get; }
    }
}
