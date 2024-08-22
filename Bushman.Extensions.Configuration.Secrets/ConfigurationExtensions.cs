using System;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.Services;
using Microsoft.Extensions.Configuration;

namespace Bushman.Extensions.Configuration.Secrets {
    public static class ConfigurationExtensions {
        public static void ExpandSecrets(this IConfiguration configuration, ISecretFactory secretFactory) {

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (secretFactory == null) throw new ArgumentNullException(nameof(secretFactory));

            foreach (var subsection in configuration.GetChildren()) {
                ExpandSectionSecrets(subsection, secretFactory);
            }
        }

        private static void ExpandSectionSecrets(IConfigurationSection section, ISecretFactory secretFactory) {

            if (section == null) throw new ArgumentNullException(nameof(section));
            if (secretFactory == null) throw new ArgumentNullException(nameof(secretFactory));

            if (!string.IsNullOrWhiteSpace(section.Value)) {
                IEncryptor encryptor = secretFactory.CreateEncryptor();
                section.Value = encryptor.Expand(section.Value);
            }

            foreach (var subsection in section.GetChildren()) {
                ExpandSectionSecrets(subsection, secretFactory);
            }
        }
    }
}
