using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using Bushman.Secrets.Test;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Bushman.Extensions.Configuration.Secrets.Test {
    [TestClass]
    public class ConfigurationExtensionTest {

        [TestMethod]
        public void TestMethod1() {

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();

            var value = "Hello World";

            // Получение фабрики секретов...

            // Вариант #1: по имени сборки. Этот способ подходит только если в сборке определено не более одной фабрики секретов.
            // ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets");

            // Вариант #2: по имени сборки и полному имени конкретного публичного класса фабрики.
            ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets", "Bushman.Secrets.Services.SecretFactory");

            IEncryptor encryptor = secretFactory.CreateEncryptor();

            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateEncryptorOptions(secretFactory), value, false);
            ISecret encryptedSecret = encryptor.Encrypt(secret);

            var text = $@"{{
                    ""prop1"": ""{value}"",
                    ""prop2"": {{ ""prop2.1"": ""{secret}"" }},
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

            configRoot.ExpandSecrets(secretFactory);
        }
    }
}
