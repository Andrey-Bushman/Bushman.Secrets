using System;

namespace Bushman.Secrets.Abstractions.Exceptions {
    /// <summary>
    /// Исключение, генерируемое в случае отсутствия информации в настройках секрета.
    /// </summary>
    public sealed class EmptyOptionsException : Exception {

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public EmptyOptionsException() : base("Невалидное строковое представление секрета: отсутствуют настройки шифрования данных.") { }
    }
}
