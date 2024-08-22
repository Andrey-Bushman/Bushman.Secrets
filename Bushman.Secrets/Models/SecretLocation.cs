using Bushman.Secrets.Abstractions.Models;
using System;

namespace Bushman.Secrets.Models {
    /// <summary>
    /// Информация о размещении секрета в тексте.
    /// </summary>
    public sealed class SecretLocation : ISecretLocation {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="startIndex">Индекс, с которого начинается секрет в тексте.</param>
        /// <param name="endIndex">Индекс, следующий за окончанием секрета в тексте.</param>
        /// <param name="secret">Секрет, размещённый в тексте.</param>
        /// <exception cref="ArgumentException">Значение индекса меньше нуля.</exception>
        /// <exception cref="ArgumentNullException">Вместо секрета указан null.</exception>
        public SecretLocation(int startIndex, int endIndex, ISecret secret) {

            if (startIndex < 0) throw new ArgumentException($"Значение параметра {nameof(startIndex)} не должно быть меньше нуля.");
            if (endIndex < 0) throw new ArgumentException($"Значение параметра {nameof(endIndex)} не должно быть меньше нуля.");
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            StartIndex = startIndex;
            EndIndex = endIndex;
            Secret = secret;
        }

        /// <summary>
        /// Индекс, с которого начинается секрет в тексте.
        /// </summary>
        public int StartIndex { get; }
        /// <summary>
        /// Индекс, следующий за окончанием секрета в тексте.
        /// </summary>
        public int EndIndex { get; }
        /// <summary>
        /// Секрет, размещённый в тексте.
        /// </summary>
        public ISecret Secret { get; }
    }
}
