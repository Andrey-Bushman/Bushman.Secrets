
# Bushman.Secrets.Abstractions

## Причина создания

В приложениях, написанных на **.Net Framework**, конфигурационные файлы обычно имеют формат XML. "Из коробки"
эта платформа предоставляет возможность шифровать и расшифровывать секции в подобных файлах, дабы скрывать или
отображать секреты: логины, пароли, строки подключения и URL различных сервисов.

Однако порой возникает необходимость хранить зашифрованные секреты и в других местах: в JSON-файлах конфигураций,
в SQL базах данных, в записях CRM, в реестре Windows, а порой они могут понадобиться и в логах. Помимо
этого, может возникать необходимость отправлять зашифрованные секреты по почте или через мессенджеры,
а так же сохранять их в git-репозиториях (например, в составе конфигурационных файлов).

В приложениях, написанных на **.NET** (в отличии от **.Net Framework**), конфигурационные файлы обычно имеют
формат JSON. "Из коробки" эта платформа не предоставляет возможность шифровать и расшифровывать секции в таких
файлах. Вместо этого Microsoft рекомендует для хранения секретов использовать такие платные хранилища секретов,
как `Azure Key Vault` или `HashiCorp Vault`.

Пакет `Bushman.Secrets.Abstractions` предоставляет абстрактную модель для создания, парсинга, шифрования, расшифровки
и _распаковки_ секретов в тексте. Реализация этой абстрактной модели находится в пакете `Bushman.Secrets`.
Все операции шифрования и расшифровки выполняются на основе сертификатов.

Для того, чтобы при запуске приложения в объектной модели конфигурационных настроек, представленной интерфейсами
`Microsoft.Extensions.Configuration.IConfigurationRoot` или `Microsoft.Extensions.Configuration.IConfiguration`
выполнить распаковку всех секретов в памяти, можно использовать пакет `Bushman.Extensions.Configuration.Secrets`,
в составе которого для интерфейса `Microsoft.Extensions.Configuration.IConfiguration` определён метод расширения
`ExpandSecrets()` (см. ниже раздел _Распаковка секретов в настройках приложения_).

## О форме записи секретов 

Секреты записываются в особом формате, позволяющем без проблем идентифицировать их в тексте. Каждый секрет может находиться в одном из
двух состояний: в расшифрованном или зашифрованном.

При использовании пакета `Bushman.Secrets`, JSON-файл с записанными в нём секретами как в качестве непосредственных значений (см. `prop2`
и `prop4`), так и в составе произвольного текста (см. `prop3` и `prop5`), может выглядеть, например, так:

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

_Распаковкой_ секрета называется его замена в тексте на хранящееся в нём расшифрованное значение. Например,
если в свойствах `prop2` и `prop4` приведённого выше JSON-файла выполнить _распаковку_ секретов, то значения
этих свойств станут таким же, как у свойства `prop1`.

Общая схема записи секрета в тексте следующая:

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

## Примеры работы с секретами

В качестве наглядных примеров, демонстрирующих работу с секретами, ниже приведены юнит-тесты.

Вспомогательный класс, для использования в приведённых ниже юнит-тестах:

```
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
        public static ISecretOptions CreateSecretOptions(ISecretFactory secretFactory) {

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
        /// <param name="content">Текст, в котором необходимо выполнить поиск и подсчёт количества значений (value).</param>
        /// <param name="value">Фрагмент текста, количество вхождений которого в исходном тексте (content) нужно посчитать.</param>
        /// <returns>Количество вхождений указанного фрагмента (value) в исходном тексте (content).</returns>
        public static int GetValuesCount(string content, string value) => new Regex(value).Matches(content).Count;
    }
}
```

Юнит-тесты по работе с секретами:

```
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bushman.Secrets.Test {
    [TestClass]
    public sealed class EncryptorTest {

        [TestMethod]
        [DataRow("Hello, World!")]
        [DataRow("|Hello|World|")]
        [DataRow("")]
        [DataRow("|")]
        [DataRow("|||")]
        [DataRow("   ")]
        public void Secret_operations_work_fine(string value) {

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            // Получение фабрики секретов...

            string assemblyName = "Bushman.Secrets";
            string className = "Bushman.Secrets.Services.SecretFactory";

            // Вариант #1: по имени сборки. ВНИМАНИЕ! Этот способ подходит только если в сборке определено
            // не более одной фабрики секретов!
            ISecretFactory secretFactory1 = secretFactoryProvider.CreateSecretFactory(assemblyName);

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory2 = secretFactoryProvider.CreateSecretFactory(assemblyName, className);

            ISecretFactory secretFactory = secretFactory2;

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            // Создаём незашифрованный секрет.
            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(TestHelper.CreateSecretOptions(secretFactory), value);

            // Создаём зашифрованный секрет.
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            Assert.AreNotEqual(decryptedSecret.ToString(), encryptedSecret.ToString());

            // Расшифровываем секрет.
            ISecret decryptedSecret2 = encryptor.Decrypt(decryptedSecret);

            Assert.AreEqual(decryptedSecret.ToString(), decryptedSecret2.ToString());

            // Распаковываем секрет.
            string expandedValue = encryptor.Expand(encryptedSecret);

            Assert.AreEqual(value, expandedValue);

            // Получаем строковое представление секрета.
            string encryptedSecretString = encryptedSecret.ToString();

            Assert.IsTrue(encryptor.IsEncryptedSecret(encryptedSecretString));
            Assert.IsFalse(encryptor.IsDecryptedSecret(encryptedSecretString));
            Assert.IsTrue(encryptor.IsSecret(encryptedSecretString));

            // Выполняем парсинг строкового представления секрета.
            ISecret parsedSecret = encryptor.ParseSecret(encryptedSecretString);

            Assert.AreEqual(encryptedSecretString, parsedSecret.ToString());
        }

        [TestMethod]
        public void RSAEncryptor_process_strings_with_secrets_correctly() {

            string value = "Hello World";

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            // Получение фабрики секретов...

            string assemblyName = "Bushman.Secrets";
            string className = "Bushman.Secrets.Services.SecretFactory";

            // Вариант #1: по имени сборки. ВНИМАНИЕ! Этот способ подходит только если в сборке определено
            // не более одной фабрики секретов!
            ISecretFactory secretFactory1 = secretFactoryProvider.CreateSecretFactory(assemblyName);

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory2 = secretFactoryProvider.CreateSecretFactory(assemblyName, className);

            ISecretFactory secretFactory = secretFactory2;

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            // Создаём расшифрованный секрет.
            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(TestHelper.CreateSecretOptions(secretFactory), value);
            // Создаём зашифрованный секрет.
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            // Тестовая строка, содержащая распакованное значение,
            // а так же расшифрованный и зашифрованный секреты.
            string text = $@"{{
                ""prop1"": ""{value}"",
                ""prop2"": ""{decryptedSecret}"",
                ""prop3"": ""{encryptedSecret}""
            }}";

            Assert.AreEqual(2, TestHelper.GetValuesCount(text, value));
            Assert.AreEqual(1, encryptor.GetEncryptedSecretsCount(text));
            Assert.AreEqual(1, encryptor.GetDecryptedSecretsCount(text));

            var encryptedText = encryptor.Encrypt(text);

            Assert.AreEqual(1, TestHelper.GetValuesCount(encryptedText, value));
            Assert.AreEqual(2, encryptor.GetEncryptedSecretsCount(encryptedText));
            Assert.AreEqual(0, encryptor.GetDecryptedSecretsCount(encryptedText));

            var decryptedText = encryptor.Decrypt(text);

            Assert.AreEqual(3, TestHelper.GetValuesCount(decryptedText, value));
            Assert.AreEqual(0, encryptor.GetEncryptedSecretsCount(decryptedText));
            Assert.AreEqual(2, encryptor.GetDecryptedSecretsCount(decryptedText));

            var expandedText = encryptor.Expand(text);

            Assert.AreEqual(3, TestHelper.GetValuesCount(expandedText, value));
            Assert.AreEqual(0, encryptor.GetEncryptedSecretsCount(expandedText));
            Assert.AreEqual(0, encryptor.GetDecryptedSecretsCount(expandedText));

            INodeCollection nodes = encryptor.ParseToNodes(text);
            string text2 = nodes.ToString();

            Assert.AreEqual(text, text2);
            Assert.AreEqual(5, nodes.Count);
        }
    }
}
```

Юнит-тесты по созданию и использованию пользовательских настроек секретов:

```
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Models;
using Bushman.Secrets.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bushman.Secrets.Test {
    [TestClass]
    public sealed class SecretOptionsTest {
        [TestMethod]
        public void Custom_SecretOptions_work_fine() {

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            // Получение фабрики секретов...

            // Вариант #1: по имени сборки. Этот способ подходит только если в сборке определено не более одной фабрики секретов.
            ISecretFactory secretFactory1 = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets");

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory2 = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets", "Bushman.Secrets.Services.SecretFactory");

            ISecretFactory secretFactory = secretFactory2;

            // Получение базовых настроек секретов...

            // Вариант #1: Получение базовых настроек, используемых по умолчанию.
            ISecretOptionsBase optionsBase1 = secretFactory.CreateSecretOptionsBase();

            // Вариант #2: Результат тот же, что и у варианта #1.
            ISecretOptionsBase optionsBase2 = secretFactory2.CreateSecretOptionsBase(
                SecretOptionsBase.DefaultEncoding,
                SecretOptionsBase.DefaultFieldSeparator,
                SecretOptionsBase.DefaultEncryptedTagPair,
                SecretOptionsBase.DefaultDecryptedTagPair);

            // Вариант #3: Создать пользовательские базовые настройки, отличные от базовых настроек, используемых по умолчанию.

            Encoding encoding = Encoding.UTF32;
            char fieldSeparator = ':';
            ITagPair encryptedTagPair = secretFactory.CreateTagPair("!!LOCKED", "LOCKED!!");
            ITagPair decryptedTagPair = secretFactory.CreateTagPair("!!UNLOCKED", "UNLOCKED!!");

            ISecretOptionsBase optionsBase3 = secretFactory.CreateSecretOptionsBase(encoding, fieldSeparator, encryptedTagPair, decryptedTagPair);

            ISecretOptionsBase optionsBase = optionsBase3;

            // Выбираем первый попавшийся сертификат из локального хранилища

            var storeLocation = StoreLocation.CurrentUser;

            string thumbprint = null; // Отпечаток сертификата.

            using (X509Store store = new X509Store(storeLocation)) {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 certificate = store.Certificates[0];
                store.Close();

                using (certificate) {
                    thumbprint = certificate.Thumbprint;
                }
            }

            // Формируем настройки для работы с секретами.

            // Вариант #1: формирование настроек на основе базовых настроек, используемых по умолчанию.

            ISecretOptions options1 = secretFactory.CreateSecretOptions(storeLocation, HashAlgorithmName.SHA1, thumbprint);

            // Вариант #2: формирование настроек на основе пользовательских базовых настроек.

            ISecretOptions options2 = secretFactory.CreateSecretOptions(optionsBase, storeLocation, HashAlgorithmName.SHA1, thumbprint);

            ISecretOptions options = options2;

            string value = "Hello World";

            // Создаём незашифрованный секрет.
            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(options, value);

            Assert.AreEqual(options, decryptedSecret.Options);
            Assert.AreEqual(options.OptionsBase, decryptedSecret.Options.OptionsBase);
            Assert.AreEqual(value, decryptedSecret.Data);
            Assert.IsFalse(decryptedSecret.IsEncrypted);

            // Создаём экземпляр объекта, с помощью которого можно шифровать, расшифровывать, распаковывать, валидировать и парсить секреты.

            // Вариант #1: Создать экземпляр IEncryptor на основе базовых настроек, используемых по умолчанию.
            IEncryptor encryptor1 = secretFactory.CreateEncryptor();

            // Вариант #2: Создать экземпляр IEncryptor на основе пользовательских базовых настроек.
            IEncryptor encryptor2 = secretFactory.CreateEncryptor(optionsBase);

            IEncryptor encryptor = encryptor2;

            // Шифруем ранее созданный не зашифрованный секрет
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            Assert.AreEqual(decryptedSecret.Options, encryptedSecret.Options);
            Assert.AreEqual(decryptedSecret.Options.OptionsBase, encryptedSecret.Options.OptionsBase);
            Assert.AreNotEqual(decryptedSecret.Data, encryptedSecret.Data);
            Assert.IsTrue(encryptedSecret.IsEncrypted);

            string encryptedSecretString = encryptedSecret.ToString();
            string decryptedSecretString = decryptedSecret.ToString();

            Assert.AreNotEqual(encryptedSecretString, decryptedSecretString);
        }
    }
}
```

## Распаковка секретов в настройках приложений

В качестве наглядных примеров, демонстрирующих работу с секретами, ниже приведены юнит-тесты.

Вспомогательный класс, для использования в приведённых ниже юнит-тестах:

```
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
        public static ISecretOptions CreateSecretOptions(ISecretFactory secretFactory) {

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
        /// <param name="content">Текст, в котором необходимо выполнить поиск и подсчёт количества значений (value).</param>
        /// <param name="value">Фрагмент текста, количество вхождений которого в исходном тексте (content) нужно посчитать.</param>
        /// <returns>Количество вхождений указанного фрагмента (value) в исходном тексте (content).</returns>
        public static int GetValuesCount(string content, string value) => new Regex(value).Matches(content).Count;
    }
}
```

Юнит-тест с примером того, как следует распаковывать секреты, хранящиеся в конфигурационных настройках приложения:

```
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using Bushman.Secrets.Test;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Bushman.Extensions.Configuration.Secrets.Test {
    [TestClass]
    public sealed class ConfigurationExtensionTest {

        [TestMethod]
        public void TestMethod1() {

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            var value = "Hello World";

            // Получение фабрики секретов...

            string assemblyName = "Bushman.Secrets";
            string className = "Bushman.Secrets.Services.SecretFactory";

            // Вариант #1: по имени сборки. ВНИМАНИЕ! Этот способ подходит только если в сборке определено
            // не более одной фабрики секретов!
            ISecretFactory secretFactory1 = secretFactoryProvider.CreateSecretFactory(assemblyName);

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory2 = secretFactoryProvider.CreateSecretFactory(assemblyName, className);

            ISecretFactory secretFactory = secretFactory2;

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            ISecret decryptedSecret = secretFactory.CreateDecryptedSecret(TestHelper.CreateSecretOptions(secretFactory), value);
            ISecret encryptedSecret = encryptor.Encrypt(decryptedSecret);

            var text = $@"{{
                    ""prop1"": ""{value}"",
                    ""prop2"": {{ ""prop2.1"": ""{decryptedSecret}"" }},
                    ""prop3"": [ {{ ""prop3.1"": ""{encryptedSecret}"" }} ]
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

            configRoot.ExpandSecrets(secretFactory); // Распаковываем все секреты в настройках приложения.

            // Теперь все секреты в configRoot распакованы!

            // ...
        }
    }
}
```
