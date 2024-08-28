
# Bushman.Secrets.Abstractions

Абстрактная модель для работы с секретами в составе текста.

## Назначение пакета

Предоставление уровня абстракции для шифрования и расшифровки секретов в тексте.

В приложениях, написанных на **.Net Framework**, конфигурационные файлы обычно имеют формат XML. "Из коробки"
эта платформа предоставляет возможность шифровать и расшифровывать секции в подобных файлах, дабы скрывать или
отображать секреты: логины, пароли, строки подключения и URL различных сервисов.

Однако порой возникает необходимость хранить зашифрованные секреты и в других местах: в произвольных текстовых
файлах или в JSON-файлах, в SQL базах данных, в записях CRM, в реестре Windows, а порой они могут понадобиться
и в логах. Помимо этого, может возникать необходимость отправлять зашифрованные секреты по почте или через
мессенджеры, а так же сохранять их в git-репозиториях (например, в составе конфигурационных файлов).

В приложениях, написанных на **.NET** (в отличии от **.Net Framework**), конфигурационные файлы обычно имеют
формат JSON. "Из коробки" эта платформа не предоставляет возможность шифровать и расшифровывать секции в таких
файлах. Вместо этого Microsoft рекомендует для хранения секретов использовать такие платные хранилища секретов,
как [Azure Key Vault](https://azure.microsoft.com/en-us/products/key-vault) или
[HashiCorp Vault](https://www.vaultproject.io/).

## Абстрактная модель секретов

Пакет [Bushman.Secrets.Abstractions](https://www.nuget.org/packages/Bushman.Secrets.Abstractions) предоставляет
простую абстрактную модель для создания, парсинга, шифрования, расшифровки и _распаковки_ секретов непосредственно
в произвольном тексте.

## Реализация абстрактной модели секретов

Реализацию этой модели предоставляет пакет [Bushman.Secrets](https://www.nuget.org/packages/Bushman.Secrets).

Все операции шифрования и расшифровки выполняются на основе сертификатов, установленных на локальной машине в
хранилищах сертификатов.

Для того, чтобы при запуске приложения в объектной модели конфигурационных настроек, представленной интерфейсами
[Microsoft.Extensions.Configuration.IConfigurationRoot](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfigurationroot?view=net-8.0)
или [Microsoft.Extensions.Configuration.IConfiguration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration?view=net-8.0)
выполнить распаковку всех секретов в памяти, можно использовать пакет [Bushman.Extensions.Configuration.Secrets](https://www.nuget.org/packages/Bushman.Extensions.Configuration.Secrets),
в составе которого определён метод расширения `ExpandSecrets()` для интерфейса
[Microsoft.Extensions.Configuration.IConfiguration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration?view=net-8.0).

## Секреты

Для того, чтобы секреты можно было распознать в тексте, они записываются в специальном формате. Каждый секрет может находиться в одном из
двух состояний: _расшифрованном_ или _зашифрованном_.

Пример текстового JSON-файла, с секретами, записанными как в качестве непосредственных значений (см. `prop2`
и `prop4`), так и в составе произвольного текстового значения (см. `prop3` и `prop5`):

```
{
  "prop1": "Hello World", // Распакованное значение.
  "prop2": "%%DECRYPTED|CurrentUser|SHA512|00DD37AA6E8AA22E9B11DFC6B8B5DD9706D9FD8C|Hello World|DECRYPTED%%", // Секрет в расшифрованном состоянии.
  "prop3": "Расшифрованный секрет в составе произвольного текста: %%DECRYPTED|CurrentUser|SHA512|00DD37AA6E8AA22E9B11DFC6B8B5DD9706D9FD8C|Hello World|DECRYPTED%%. Мама мыла раму.",

  // Секрет в зашифрованном состоянии.
  "prop4": "%%ENCRYPTED|CurrentUser|SHA512|00DD37AA6E8AA22E9B11DFC6B8B5DD9706D9FD8C|cFyOsNujOBp21frIVpIwMT2hjzR6ZDsAtZfs8eWfoVcLiqDqEO+rAEXVmE6KbQMLv+pizS8O/Ri124uM7YvM8NbsKfP2AQI4G/reup5I8kmpGXGkVjevuDuQ0eo5MRbobBPIXPFtvja9zCFn3hpNk/rt243vGMCbhCdIRgXRyOGrHxNuxlB7wHDEkZ+cz68D5cLLYYTF2ctpvgqMHjU7DRg5Vm5NT3N+Rn1FuAFmTa1laBm+Db5CM3yQ1M376FbEU6fiW3xnVrd7i52BREo4T80asmjFLcIxR8R7j5nBpZcSCM4e+wmD6IJGjJDh9Pc79I/s5P2bQduczJIxWIS1mQ==|ENCRYPTED%%",
  "prop5": "Зашифрованный секрет в составе произвольного текста: %%ENCRYPTED|CurrentUser|SHA512|00DD37AA6E8AA22E9B11DFC6B8B5DD9706D9FD8C|cFyOsNujOBp21frIVpIwMT2hjzR6ZDsAtZfs8eWfoVcLiqDqEO+rAEXVmE6KbQMLv+pizS8O/Ri124uM7YvM8NbsKfP2AQI4G/reup5I8kmpGXGkVjevuDuQ0eo5MRbobBPIXPFtvja9zCFn3hpNk/rt243vGMCbhCdIRgXRyOGrHxNuxlB7wHDEkZ+cz68D5cLLYYTF2ctpvgqMHjU7DRg5Vm5NT3N+Rn1FuAFmTa1laBm+Db5CM3yQ1M376FbEU6fiW3xnVrd7i52BREo4T80asmjFLcIxR8R7j5nBpZcSCM4e+wmD6IJGjJDh9Pc79I/s5P2bQduczJIxWIS1mQ==|ENCRYPTED%%. Мама мыла раму."
}
```

_Распаковкой_ секрета называется его замена в тексте на хранящееся в нём _расшифрованное_ значение. Например,
если в свойствах `prop2` и `prop4` приведённого выше JSON-файла выполнить _распаковку_ секретов, то значения
этих свойств станут таким же, как у свойства `prop1`.

### Структура секрета

Общая структура записи секрета в тексте следующая:

```
SecretOpenTag|SecretStorage|HashAlgorithmName|Thumbprint|Data|SecretCloseTag
```

где:

  * `SecretOpenTag` - тег открытия секрета. Если секрет зашифрован, то это будет тег `%%ENCRYPTED`. Если расшифрован, то `%%DECRYPTED`.
  * `SecretStorage` - хранилище, в котором находится нужный сертификат. Допустимые значения: `LocalMachine` и `CurrentUser`.
  * `HashAlgorithmName` - наименование алгоритма хеширования. Допустимые значения: `MD5`, `SHA1`, `SHA256`, `SHA384`, `SHA512`. 
  * `Thumbprint` - отпечаток сертификата, с помощью ключей которого следует выполнять шифрование и расшифровку секрета.
    Это значение можно посмотреть в настройках сертификата на вкладке "Состав".
  * `Data` - данные, сохраняемые в секрете. Если секрет не зашифрован, то этими данными будет обычный текст.
    Если секрет зашифрован, то в качестве значения будут записаны зашифрованные данные в формате строки `base64`.
  * `SecretCloseTag` - тег закрытия секрета. Если секрет зашифрован, то это будет тег `ENCRYPTED%%`. Если расшифрован, то `DECRYPTED%%`.

В качестве разделителя полей используется `|`. Этот символ разрешено использовать в т.ч. и в тексте, сохраняемом в поле `Data`.

## Пример использования

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

            string value = "Hello, World!"; // Строка, представляющая собой конфиденциальную информацию.

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

            INodeCollection nodes = encryptor.ParseToNodes(text); // Парсим строку с секретами в коллекцию узлов.

            string text2 = nodes.ToString(); // На основе коллекции узлов формируем итоговую строку.
        }
    }
}
```
