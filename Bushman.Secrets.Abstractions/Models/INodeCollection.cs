using System.Collections.Generic;

namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Коллекция узлов.
    /// </summary>
    public interface INodeCollection : IReadOnlyCollection<INode>, IReadOnlyList<INode> {
    }
}
