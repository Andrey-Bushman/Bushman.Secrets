
# Bushman.Secrets

## О пакете

Работа с секретами в составе текста.

Реализация уровня абстракции, определённого в пакете 
[Bushman.Secrets.Abstractions](https://www.nuget.org/packages/Bushman.Secrets.Abstractions).

## Пример использования

Предварительно подключите к своему проекту следующие пакеты:

  * [Bushman.Secrets](https://www.nuget.org/packages/Bushman.Secrets)

```
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Bushman.Secrets.Abstractions.Models;

namespace ConsoleApp1 {
    internal class Program {
        static void Main(string[] args) {

            // 01. СОЗДАЁМ ПРОВАЙДЕР ФАБРИКИ СЕКРЕТОВ.

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            // 02. СОЗДАЁМ ФАБРИКУ СЕКРЕТОВ.

            string secretFactoryAssemblyName = "Bushman.Secrets";
            string secretFactoryClassName = "Bushman.Secrets.Services.SecretFactory";

            ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory(
                secretFactoryAssemblyName, secretFactoryClassName);

            // 03. СОЗДАЁМ МЕХАНИЗМ ШИФРОВАНИЯ/РАСШИФРОВКИ.

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            // 04. ВЫБИРАЕМ СЕРТИФИКАТ, КОТОРЫЙ БУДЕМ ИСПОЛЬЗОВАТЬ ДЛЯ ШИФРОВАНИЯ И РАСШИФРОВКИ.

            StoreLocation storeLocation = StoreLocation.CurrentUser; // Хранилище сертификатов.

            string thumbprint = null; // Отпечаток интересующего нас сертификата.

            using (X509Store store = new X509Store(storeLocation)) {
                store.Open(OpenFlags.ReadOnly);

                // Для нашего примера берём первый попавшийся сертификат.
                X509Certificate2 certificate = store.Certificates[0];

                store.Close();

                using (certificate) {
                    thumbprint = certificate.Thumbprint;
                }
            }

            // 05. ФОРМИРУЕМ НАСТРОЙКИ ДЛЯ РАБОТЫ С СЕКРЕТАМИ.

            ISecretOptions secretOptions = secretFactory.CreateSecretOptions(storeLocation,
                HashAlgorithmName.SHA1, thumbprint);

            // ПРИМЕРЫ БАЗОВЫХ ОПЕРАЦИЙ С СЕКРЕТАМИ.
            // ----------------------------------------------------------------------------------

            // Строка, представляющая собой конфиденциальную информацию.
            string value = "Hello, World!";

            // Создаём незашифрованный секрет.
            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(secretOptions, value);

            // Шифруем секрет.
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            // Расшифровываем секрет.
            ISecret decryptedSecret2 = encryptor.Decrypt(decryptedSecret);

            // Распаковываем секрет.
            string expandedValue = encryptor.Expand(encryptedSecret);

            // Получаем строковое представление секрета.
            string encryptedSecretString = encryptedSecret.ToString();

            // Выполняем парсинг строкового представления секрета.
            ISecret parsedSecret = encryptor.ParseSecret(encryptedSecretString);

            bool isSecret = encryptor.IsSecret(encryptedSecretString); // true
            bool isEncryptedSecret = encryptor.IsEncryptedSecret(encryptedSecretString); // true
            bool isDecryptedSecret = encryptor.IsDecryptedSecret(encryptedSecretString); // false

            // ПРИМЕРЫ БАЗОВЫХ ОПЕРАЦИЙ С СЕКРЕТАМИ, НАХОДЯЩИМИСЯ В СОСТАВЕ ПРОИЗВОЛЬНОГО ТЕКСТА.
            // ----------------------------------------------------------------------------------

            // Тестовая строка, содержащая распакованное значение, а так же расшифрованный и
            // зашифрованный секреты с этим же значением.
            string text = $@"{{
                ""prop1"": ""{value}"",
                ""prop2"": ""{decryptedSecret}"",
                ""prop3"": ""{encryptedSecret}""
            }}";

            string encryptedText = encryptor.Encrypt(text); // Зашифровываем все секреты в тексте.

            string decryptedText = encryptor.Decrypt(text); // Расшифровываем все секреты в тексте.

            string expandedText = encryptor.Expand(text); // Распаковываем все секреты в тексте.

            // Парсим строку с секретами в коллекцию узлов.
            INodeCollection nodes = encryptor.ParseToNodes(text);

            string text2 = nodes.ToString(); // На основе коллекции узлов формируем итоговую строку.
        }
    }
}
```
