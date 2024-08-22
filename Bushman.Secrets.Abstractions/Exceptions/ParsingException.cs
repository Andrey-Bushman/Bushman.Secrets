using System;

namespace Bushman.Secrets.Abstractions.Exceptions {
    /// <summary>
    /// Ошибка парсинга.
    /// </summary>
    public sealed class ParsingException : Exception {
        public ParsingException(string message) : base(message) { }
    }
}
