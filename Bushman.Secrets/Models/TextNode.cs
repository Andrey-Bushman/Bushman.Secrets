using Bushman.Secrets.Abstractions.Models;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Узел текста.
    /// </summary>
    public sealed class TextNode : ITextNode {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="index">Порядковый номер узла в коллекциии.</param>
        /// <param name="value">Значение текста.</param>
        public TextNode(int index, string value) {

            NodeType = NodeType.Text;
            Index = index;
            Value = value;
        }
        /// <summary>
        /// Тип узла.
        /// </summary>
        public NodeType NodeType { get; }
        /// <summary>
        /// Порядковый номер узла в коллекциии.
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// Значение текста.
        /// </summary>
        public string Value { get; }
    }
}
