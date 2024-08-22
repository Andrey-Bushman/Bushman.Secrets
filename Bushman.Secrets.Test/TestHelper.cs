using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bushman.Secrets.Abstractions.Services;
using System;
using Bushman.Secrets.Abstractions.Models;

namespace Bushman.Secrets.Test {
    /// <summary>
    /// Статический класс, предоставляющий набор вспомогательных методов, используемых в тестах.
    /// </summary>
    public static class TestHelper {
        /// <summary>
        /// Создать экземпляр ISecretOptions на основе произвольного сертификата, доступного текущему
        /// пользователю в его локальном хранилище.
        /// </summary>
        /// <param name="secretFactory">Фабрика секретов.</param>
        /// <returns>Экземпляр ISecretOptions.</returns>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public static ISecretOptions CreateEncryptorOptions(ISecretFactory secretFactory) {

            if (secretFactory == null) throw new ArgumentNullException(nameof(secretFactory));

            var storeLocation = StoreLocation.CurrentUser;

            using (X509Store store = new X509Store(storeLocation)) {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 certificate = store.Certificates[0];
                store.Close();

                using (certificate) {
                    return secretFactory.CreateSecretOptions(storeLocation, HashAlgorithmName.SHA512, certificate.Thumbprint);
                }
            }
        }

        /// <summary>
        /// Посчитать в исходном тексте (content) общее количество повторений некоторого фрагмента текста (value).
        /// </summary>
        /// <param name="content">Текст, в котором необходимо выполнить поиск и подсчёт.</param>
        /// <param name="value">Фрагмент текста, количество вхождений которого в исходном тексте (content) нужно посчитать.</param>
        /// <returns>Количество вхождений указанного фрагмента (value) текста в исходный текст (content).</returns>
        public static int GetValuesCount(string content, string value) => new Regex(value).Matches(content).Count;
    }
}
