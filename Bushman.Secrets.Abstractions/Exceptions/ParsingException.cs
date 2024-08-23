using System;

namespace Bushman.Secrets.Abstractions.Exceptions {
    /// <summary>
    /// Ошибка парсинга.
    /// </summary>
    public sealed class ParsingException : Exception {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        public ParsingException(string message) : base(message) { }
    }
}
