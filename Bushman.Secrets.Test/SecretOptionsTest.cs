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
