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

            var value = "Hello World";

            ISecretFactory secretFactory = new SecretFactory();
            IEncryptor encryptor = secretFactory.CreateEncryptor();

            ISecret secret = secretFactory.CreateSecret(TestHelper.CreateRSAEncryptorOptions(secretFactory), value, false);
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

            var configRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonStream(stream)
                .Build();

            configRoot.ExpandSecrets(secretFactory);
        }
    }
}
