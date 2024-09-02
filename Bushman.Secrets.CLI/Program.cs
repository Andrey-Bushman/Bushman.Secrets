using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Abstractions.Services;
using Bushman.Secrets.CLI.Models;
using Bushman.Secrets.Models;
using Bushman.Secrets.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

internal class Program {

    /// <summary>
    /// Имена параметров, допустимых при вызове приложения.
    /// </summary>
    sealed class ParameterName {
        /// <summary>
        /// Имя файла, подлежащего обработке. Параметр обязателен.
        /// </summary>
        public const string File = "file";
        /// <summary>
        /// Кодировка файла, подлежащего обработке. Параметр опционален.
        /// </summary>
        public const string Encoding = "encoding";
        /// <summary>
        /// Операция, которую следует выполнить над указанным файлом.
        /// </summary>
        public const string Operation = "operation";
    }

    static void Main(string[] args) {

        try {

            if (args.Length == 0) {
                var readmeFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "readme.md"));
                if (File.Exists(readmeFile)) {
                    Console.WriteLine(File.ReadAllText(readmeFile));
                }
                else {
                    Console.Error.WriteLine($"Файл \"{readmeFile}\" не найден.");
                }
                return;
            }

            IConfigurationRoot configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

            foreach (var arg in new[] {
                ParameterName.File,
                ParameterName.Encoding,
                ParameterName.Operation,
            }) {
                if (string.IsNullOrWhiteSpace(configRoot[arg])) throw new ArgumentNullException(nameof(arg));
            }

            ISecretFactoryProvider secretFactoryProvider = new SecretFactoryProvider();
            ISecretFactory secretFactory = secretFactoryProvider.CreateSecretFactory("Bushman.Secrets");

            ISecretOptionsBase secretOptionsBase = secretFactory.CreateSecretOptionsBase(
                Encoding.GetEncoding(configRoot[ParameterName.Encoding]!),
                SecretOptionsBase.DefaultFieldSeparator,
                SecretOptionsBase.DefaultEncryptedTagPair,
                SecretOptionsBase.DefaultDecryptedTagPair,
                256,
                CipherMode.CBC);

            IEncryptor encryptor = secretFactory.CreateEncryptor(secretOptionsBase);

            Operation operation = Enum.Parse<Operation>(configRoot[ParameterName.Operation]!, true);
            string fileFullName = Path.GetFullPath(Environment.ExpandEnvironmentVariables(configRoot[ParameterName.File]!));

            string fileContent = File.ReadAllText(fileFullName, secretOptionsBase.Encoding);

            switch (operation) {
                case Operation.Encrypt:
                    fileContent = encryptor.Encrypt(fileContent);
                    break;
                case Operation.Decrypt:
                    fileContent = encryptor.Decrypt(fileContent);
                    break;
                case Operation.Expand:
                    fileContent = encryptor.Expand(fileContent);
                    break;
                default:
                    throw new ApplicationException($"Неизвестная операция: {operation}.");
            }

            File.WriteAllText(fileFullName, fileContent, secretOptionsBase.Encoding);

            Console.WriteLine($"Операция {operation} выполнена. Содержимое файла \"{fileFullName}\" обновлено.");
        }
        catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
        }
    }
}