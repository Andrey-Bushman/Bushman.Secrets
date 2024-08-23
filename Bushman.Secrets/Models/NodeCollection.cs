using Bushman.Secrets.Abstractions.Exceptions;
using Bushman.Secrets.Abstractions.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Коллекция узлов.
    /// </summary>
    public sealed class NodeCollection : INodeCollection {
        internal NodeCollection(INode[] nodes) {

            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            _nodes = new List<INode>(nodes);
        }

        private readonly IReadOnlyList<INode> _nodes;
        /// <summary>
        /// Количество узлов в коллекции.
        /// </summary>
        public int Count => _nodes.Count;

        /// <summary>
        /// Получить узел по его индексу в коллекции.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public INode this[int index] => _nodes[index];

        /// <summary>
        /// Получить обобщённый перечислитель элементов INode.
        /// </summary>
        /// <returns>Обобщённый перечислитель элементов INode.</returns>
        public IEnumerator<INode> GetEnumerator() => _nodes.GetEnumerator();

        /// <summary>
        /// Получить перечислитель.
        /// </summary>
        /// <returns>Перечислитель.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();

        /// <summary>
        /// Получить строковое представление коллекции узлов.
        /// </summary>
        /// <returns>Строковое представление коллекции узлов.</returns>
        /// <exception cref="ParsingException">Если хотя бы один из элементов INode имеет тип, отличный от NodeType.Text или NodeType.Secret.</exception>
        public override string ToString() {

            var sb = new StringBuilder();

            foreach (var node in _nodes.OrderBy(n => n.Index)) {

                if (node.NodeType == NodeType.Text) {

                    var textNode = (ITextNode)node;
                    sb.Append(textNode.Value);
                }
                else if (node.NodeType == NodeType.Secret) {

                    var secretNode = (ISecretNode)node;
                    sb.Append(secretNode.Value.Secret);
                }
                else {
                    throw new ParsingException($"Не ожиданное значение перечисления {nameof(NodeType)}: {node.NodeType}.");
                }
            }

            return sb.ToString();
        }
    }
}
