
namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Текстовый узел.
    /// </summary>
    public interface ITextNode : INode {
        /// <summary>
        /// Значение текста.
        /// </summary>
        string Value { get; }
    }
}
