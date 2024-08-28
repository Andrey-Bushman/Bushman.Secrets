
# Bushman.Extensions.Configuration.Secrets

Распаковка [секретов](https://www.nuget.org/packages/Bushman.Secrets.Abstractions)
в конфигурационных настройках приложения.

## О пакете

Расширение для распаковки [секретов](https://www.nuget.org/packages/Bushman.Secrets.Abstractions)
в настройках приложения, представленных экземпляром
[Microsoft.Extensions.Configuration.IConfiguration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration?view=net-8.0).

Для работы с секретами используется пакет [Bushman.Secrets](https://www.nuget.org/packages/Bushman.Secrets),
реализующий абстрактную модель, определённую в пакете
[Bushman.Secrets.Abstractions](https://www.nuget.org/packages/Bushman.Secrets.Abstractions).

## Пример использования

Предварительно подключите к своему проекту следующие пакеты:

  * [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json)
  * [Microsoft.Extensions.Configuration.Binder](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder)
  * [Bushman.Secrets](https://www.nuget.org/packages/Bushman.Secrets)

```
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Bushman.Secrets.Abstractions.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using Bushman.Extensions.Configuration.Secrets;

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

            // Строка, представляющая собой конфиденциальную информацию.
            string value = "Hello, World!";

            // Создаём незашифрованный секрет.
            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(secretOptions, value);

            // Шифруем секрет.
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            // ПРИМЕРЫ БАЗОВЫХ ОПЕРАЦИЙ С СЕКРЕТАМИ, НАХОДЯЩИМИСЯ В СОСТАВЕ ПРОИЗВОЛЬНОГО ТЕКСТА.
            // ----------------------------------------------------------------------------------

            // Тестовая строка, содержащая распакованное значение, а так же расшифрованный и
            // зашифрованный секреты с этим же значением.
            string text = $@"{{
                ""prop1"": ""{value}"",
                ""prop2"": ""{decryptedSecret}"",
                ""prop3"": ""{encryptedSecret}""
            }}";

            Stream stream = new MemoryStream(encryptor.OptionsBase.Encoding.GetByteCount(value));

            using (var writer = new StreamWriter(stream, encryptor.OptionsBase.Encoding, 1024, true)) {
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;
            }

            IConfigurationRoot configRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonStream(stream)
                .Build();

            // На данный момент времени секреты в настройках приложения всё ещё не распакованы...

            // Распаковываем все секреты в настройках приложения.
            configRoot.ExpandSecrets(secretFactory);

            // Теперь все секреты в configRoot распакованы!
        }
    }
}
```
