// TestHelper.cs
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Bushman.Secrets.Abstractions.Services;
using System;
using Bushman.Secrets.Abstractions.Models;

namespace Bushman.Secrets.Test {
    public static class TestHelper {
        public static ISecretOptions CreateEncryptorOptions(ISecretFactory secretFactory) {

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

        public static int GetValuesCount(string content, string value) => new Regex(value).Matches(content).Count;
    }
}
