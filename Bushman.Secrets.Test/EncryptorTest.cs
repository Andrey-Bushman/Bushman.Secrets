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
            ISecret decryptedSecret2 = encryptor.Decrypt(encryptedSecret);

            

            // Распаковываем секрет.
            string expandedValue = encryptor.Expand(encryptedSecret);

            var valueBytes = encryptedSecret.Options.OptionsBase.Encoding.GetBytes(value);
            var decryptedValueBytes = decryptedSecret.Options.OptionsBase.Encoding.GetBytes(decryptedSecret.Data);
            var encryptedValueBytes = encryptedSecret.Options.OptionsBase.Encoding.GetBytes(encryptedSecret.Data);
            var decryptedValueBytes2 = decryptedSecret2.Options.OptionsBase.Encoding.GetBytes(decryptedSecret2.Data);

            var expandedValueBytes = decryptedSecret.Options.OptionsBase.Encoding.GetBytes(expandedValue);

            // Получаем строковое представление секрета.
            string encryptedSecretString = encryptedSecret.ToString();

            // Выполняем парсинг строкового представления секрета.
            ISecret parsedSecret = encryptor.ParseSecret(encryptedSecretString);

            ISecret parsedSecret2 = encryptor.ParseSecret(decryptedSecret.ToString());

            Assert.AreEqual(decryptedSecret.ToString(), decryptedSecret2.ToString());
            Assert.AreEqual(value, expandedValue);

            Assert.IsTrue(encryptor.IsEncryptedSecret(encryptedSecretString));
            Assert.IsFalse(encryptor.IsDecryptedSecret(encryptedSecretString));
            Assert.IsTrue(encryptor.IsSecret(encryptedSecretString));

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
