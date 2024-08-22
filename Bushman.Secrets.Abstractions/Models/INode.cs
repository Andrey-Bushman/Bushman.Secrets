
namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Общая информация об узле.
    /// </summary>
    public interface INode {
        /// <summary>
        /// Порядковый номер узла в коллекциии.
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Тип узла.
        /// </summary>
        NodeType NodeType { get; }
    }
}
