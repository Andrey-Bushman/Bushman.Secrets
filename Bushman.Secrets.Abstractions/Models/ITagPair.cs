namespace Bushman.Secrets.Abstractions.Models {
    /// <summary>
    /// Строковые представления тегов, посредством которых секреты помечаются в тексте.
    /// </summary>
    public interface ITagPair {
        /// <summary>
        /// Открывающий тег секрета.
        /// </summary>
        string OpenTag { get; }
        /// <summary>
        /// Закрывающий тег секрета.
        /// </summary>
        string CloseTag { get; }
    }
}