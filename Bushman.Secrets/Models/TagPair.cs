using Bushman.Secrets.Abstractions.Models;
using System;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Строковые представления тегов, посредством которых секреты помечаются в тексте.
    /// </summary>
    public sealed class TagPair : ITagPair {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="openTag">Открывающий тег секрета.</param>
        /// <param name="closeTag">Закрывающий тег секрета.</param>
        /// <exception cref="ArgumentNullException">В качестве значения параметра передан null.</exception>
        public TagPair(string openTag, string closeTag) {

            if (string.IsNullOrEmpty(openTag)) throw new ArgumentNullException(nameof(openTag));
            if (string.IsNullOrEmpty(closeTag)) throw new ArgumentNullException(nameof(closeTag));

            OpenTag = openTag;
            CloseTag = closeTag;
        }
        /// <summary>
        /// Открывающий тег секрета.
        /// </summary>
        public string OpenTag { get; }
        /// <summary>
        /// Закрывающий тег секрета.
        /// </summary>
        public string CloseTag { get; }
    }
}
