using System;

namespace Bushman.Secrets.Abstractions.Exceptions {
    public sealed class EmptyOptionsException: Exception {

        public EmptyOptionsException(): base("Невалидное строковое представление секрета: отсутствуют настройки шифрования данных.") { }
    }
}
