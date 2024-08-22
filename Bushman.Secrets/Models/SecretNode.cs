using Bushman.Secrets.Abstractions.Models;
using System;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Узел секрета.
    /// </summary>
    public sealed class SecretNode : ISecretNode {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="index">Порядковый номер узла в коллекциии.</param>
        /// <param name="value">Информация о размещении секрета в тексте.</param>
        /// <exception cref="ArgumentNullException">Значение параметра равно null.</exception>
        public SecretNode(int index, ISecretLocation value) {

            if (value == null) throw new ArgumentNullException(nameof(value));

            NodeType = NodeType.Secret;
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
        /// Информация о размещении секрета в тексте.
        /// </summary>
        public ISecretLocation Value { get; }
    }
}
