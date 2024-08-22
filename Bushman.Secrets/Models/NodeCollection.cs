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

        public int Count => _nodes.Count;

        public INode this[int index] => _nodes[index];

        public IEnumerator<INode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();

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
