// EncryptorTest.cs
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bushman.Secrets.Test {
    [TestClass]
    public class EncryptorTest {

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

            // Вариант #1: по имени сборки. Этот способ подходит только если в сборке определено не более одной фабрики секретов.
            // ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets");

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets", "Bushman.Secrets.Services.SecretFactory");

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            // Создаём незашифрованный секрет.
            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateEncryptorOptions(secretFactory), value, false);

            // Создаём зашифрованный секрет.
            ISecret encryptedSecret = encryptor.Encrypt(secret);

            Assert.AreNotEqual(secret.ToString(), encryptedSecret.ToString());

            // Расшифровываем секрет.
            ISecret decryptedSecret = encryptor.Decrypt(secret);

            Assert.AreEqual(secret.ToString(), decryptedSecret.ToString());

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

            var value = "Hello World";

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            // Получение фабрики секретов...

            // Вариант #1: по имени сборки. Этот способ подходит только если в сборке определено не более одной фабрики секретов.
            // ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets");

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets", "Bushman.Secrets.Services.SecretFactory");
            IEncryptor encryptor = secretFactory.CreateEncryptor();

            // Создаём расшифрованный секрет.
            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateEncryptorOptions(secretFactory), value, false);
            // Создаём зашифрованный секрет.
            ISecret encryptedSecret = encryptor.Encrypt(secret);

            // Тестовая строка, содержащая распакованное значение,
            // а так же расшифрованный и зашифрованный секреты.
            var text = $@"{{
                ""prop1"": ""{value}"",
                ""prop2"": ""{secret}"",
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
