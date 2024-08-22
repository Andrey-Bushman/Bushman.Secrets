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

            ISecretFactory secretFactory = new SecretFactory();
            IEncryptor encryptor = secretFactory.CreateEncryptor();

            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateRSAEncryptorOptions(secretFactory), value, false);

            ISecret encryptedSecret = encryptor.Encrypt(secret);

            Assert.AreNotEqual(secret.ToString(), encryptedSecret.ToString());

            ISecret decryptedSecret = encryptor.Decrypt(secret);

            Assert.AreEqual(secret.ToString(), decryptedSecret.ToString());

            string expandedValue = encryptor.Expand(encryptedSecret);

            Assert.AreEqual(value, expandedValue);
        }

        [TestMethod]
        public void RSAEncryptor_process_strings_with_secrets_correctly() {

            var value = "Hello World";

            ISecretFactory secretFactory = new SecretFactory();
            IEncryptor encryptor = secretFactory.CreateEncryptor();

            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateRSAEncryptorOptions(secretFactory), value, false);
            ISecret encryptedSecret = encryptor.Encrypt(secret);

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
        }
    }
}
