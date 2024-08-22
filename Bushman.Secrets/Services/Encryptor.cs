using Bushman.Secrets.Abstractions.Exceptions;
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Bushman.Secrets.Services {
    /// <summary>
    /// Реализация интерфейса валидации, парсинга, шифрования и расшифровки секретов.
    /// </summary>
    public sealed class Encryptor : IEncryptor {

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Encryptor() {

            OptionsBase = new SecretOptionsBase();
        }

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="optionsBase">Базовые настройки секретов.</param>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public Encryptor(ISecretOptionsBase optionsBase) {

            if (optionsBase == null) throw new ArgumentNullException(nameof(optionsBase));

            OptionsBase = optionsBase;
        }
        /// <summary>
        /// Базовые настройки для работы с секретами.
        /// </summary>
        public ISecretOptionsBase OptionsBase { get; }
        /// <summary>
        /// Получить количество расшифрованных секретов в строке.
        /// </summary>
        /// <param name="content">Строка, для которой следует выполнить подсчёт.</param>
        /// <returns>Количество расшифрованных секретов в строке.</returns>
        public int GetDecryptedSecretsCount(string content) => new Regex(OptionsBase.DecryptedTagPair.OpenTag).Matches(content).Count;
        /// <summary>
        /// Получить количество зашифрованных секретов в строке.
        /// </summary>
        /// <param name="content">Строка, для которой следует выполнить подсчёт.</param>
        /// <returns>Количество зашифрованных секретов в строке.</returns>
        public int GetEncryptedSecretsCount(string content) => new Regex(OptionsBase.EncryptedTagPair.OpenTag).Matches(content).Count;

        public readonly Encoding Encoding = Encoding.UTF8;
        /// <summary>
        /// Выяснить, является ли полученная строка секретом (не важно, расшифрованным или зашифрованным).
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является секретом. False - не является.</returns>
        public bool IsSecret(string value) => IsEncryptedSecret(value) || IsDecryptedSecret(value);
        /// <summary>
        /// Выяснить, является ли полученная строка зашифрованным секретом.
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является зашифрованным секретом. False - не является.</returns>
        public bool IsEncryptedSecret(string value) => value == null ? false
            : value.Trim().StartsWith($"{OptionsBase.EncryptedTagPair.OpenTag}{OptionsBase.FieldSeparator}", StringComparison.InvariantCultureIgnoreCase)
            && value.Trim().EndsWith($"{OptionsBase.FieldSeparator}{OptionsBase.EncryptedTagPair.CloseTag}", StringComparison.InvariantCultureIgnoreCase);
        /// <summary>
        /// Выяснить, является ли полученная строка расшифрованным секретом.
        /// </summary>
        /// <param name="value">Строка, подлежащая проверке.</param>
        /// <returns>True - полученная строка является расшифрованным секретом. False - не является.</returns>
        public bool IsDecryptedSecret(string value) => value == null ? false
            : value.Trim().StartsWith($"{OptionsBase.DecryptedTagPair.OpenTag}{OptionsBase.FieldSeparator}", StringComparison.InvariantCultureIgnoreCase)
            && value.Trim().EndsWith($"{OptionsBase.FieldSeparator}{OptionsBase.DecryptedTagPair.CloseTag}", StringComparison.InvariantCultureIgnoreCase);
        /// <summary>
        /// Распарсить строку в экземпляр ISecret.
        /// </summary>
        /// <param name="secret">Строка, подлежащая парсингу.</param>
        /// <returns>Экземпляр ISecret.</returns>
        public ISecret ParseSecret(string secret) {

            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));

            if (!IsSecret(secret)) throw new ParsingException("Исходная строка не является строковым представлением секрета.");

            secret = secret.Trim();

            ITagPair tagPair = null;

            bool isEncrypted = IsEncryptedSecret(secret);

            if (isEncrypted) {
                tagPair = OptionsBase.EncryptedTagPair;
            }
            else {
                tagPair = OptionsBase.DecryptedTagPair;
            }

            var endTagIndex = secret.IndexOf($"{OptionsBase.FieldSeparator}{tagPair.CloseTag}");

            if (tagPair.OpenTag.Length > endTagIndex) throw new EmptyOptionsException();

            var secretInfo = secret.Substring(tagPair.OpenTag.Length + 1 /*ValueSeparator*/, endTagIndex - tagPair.OpenTag.Length - 1 /*ValueSeparator*/);
            var secretInfoValues = secretInfo.Split(OptionsBase.FieldSeparator);
            var minExpectedCount = 4;

            if (secretInfoValues.Length < minExpectedCount) throw new ParsingException(
                $"Ожидаемое количество свойств в секрете: {minExpectedCount}. Фактическое: {secretInfoValues.Length}.");

            var options = new SecretOptions(
                OptionsBase,
                (StoreLocation)Enum.Parse(typeof(StoreLocation),
                secretInfoValues[0].Trim()),
                new HashAlgorithmName(secretInfoValues[1].Trim()),
                secretInfoValues[2].Trim()
            );

            var secretData = string.Join($"{OptionsBase.FieldSeparator}", secretInfoValues.Skip(3));

            return new Secret(options, secretData, isEncrypted);
        }
        /// <summary>
        /// Зашифровать секрет.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий шифрованию.</param>
        /// <returns>Если секрет уже зашифрован, то возвращается объект, полученный в параметре secret. 
        /// В противном случае выполняется шифрование и возвращается новый экземпляр ISecret.</returns>
        public ISecret Encrypt(ISecret secret) {

            if (secret == null) throw new ArgumentNullException(nameof(secret));
            if (secret.IsEncrypted) return secret;
            else if (string.IsNullOrWhiteSpace(secret.Data)) {
                return new Secret(secret.Options, secret.Data, true);
            }

            RSAEncryptionPadding encryptionPadding = RSAEncryptionPadding.CreateOaep(secret.Options.HashAlgorithmName);
            X509Certificate2 certificate = null;

            using (X509Store store = new X509Store(secret.Options.StoreLocation)) {
                store.Open(OpenFlags.ReadOnly);
                foreach (var item in store.Certificates) {
                    if (item.Thumbprint.Equals(secret.Options.Thumbprint, StringComparison.InvariantCultureIgnoreCase)) {
                        certificate = item;
                        break;
                    }
                }
                store.Close();
            }

            if (certificate == null) throw new CryptographicException(
                $"Сертификат {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" не найден в хранилище {secret.Options.StoreLocation}.");

            using (certificate) {
                RSA rsa = certificate.GetRSAPrivateKey();

                if (rsa == null) {
                    throw new CryptographicException($"Не удалось получить приватный ключ сертификата {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" из хранилища {secret.Options.StoreLocation}.");
                }
                else {
                    using (rsa) {
                        var data = Convert.ToBase64String(rsa.Encrypt(Encoding.GetBytes(secret.Data), encryptionPadding));
                        return new Secret(secret.Options, data, true);
                    }
                }
            }
        }
        /// <summary>
        /// Расшифровать секрет.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий расшифровке.</param>
        /// <returns>Если секрет уже расшифрован, то возвращается объект, полученный в параметре secret. 
        /// В противном случае выполняется расшифровка и возвращается новый экземпляр ISecret.</returns>
        public ISecret Decrypt(ISecret secret) {

            if (secret == null) throw new ArgumentNullException(nameof(secret));
            if (!secret.IsEncrypted) return secret;
            else if (string.IsNullOrWhiteSpace(secret.Data)) {
                return new Secret(secret.Options, secret.Data, false);
            }

            RSAEncryptionPadding encryptionPadding = RSAEncryptionPadding.CreateOaep(secret.Options.HashAlgorithmName);
            X509Certificate2 certificate = null;

            using (X509Store store = new X509Store(secret.Options.StoreLocation)) {
                store.Open(OpenFlags.ReadOnly);
                foreach (var item in store.Certificates) {
                    if (item.Thumbprint.Equals(secret.Options.Thumbprint, StringComparison.InvariantCultureIgnoreCase)) {
                        certificate = item;
                        break;
                    }
                }
                store.Close();
            }

            if (certificate == null) throw new CryptographicException(
                $"Сертификат {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" не найден в хранилище {secret.Options.StoreLocation}.");

            using (certificate) {
                RSA rsa = certificate.GetRSAPrivateKey();

                if (rsa == null) {
                    throw new CryptographicException($"Не удалось получить приватный ключ сертификата {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" из хранилища {secret.Options.StoreLocation}.");
                }
                else {
                    using (rsa) {
                        var data = Encoding.GetString(rsa.Decrypt(Convert.FromBase64String(secret.Data), encryptionPadding));
                        return new Secret(secret.Options, data, false);
                    }
                }
            }
        }
        /// <summary>
        /// Зашифровать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно зашифровать все секреты.</param>
        /// <returns>Возвращается строка с зашифрованными секретами.</returns>
        public string Encrypt(string text) => UpdateText(text, Encrypt);
        /// <summary>
        /// Расшифровать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно расшифровать все секреты.</param>
        /// <returns>Возвращается строка с расшифрованными секретами.</returns>
        public string Decrypt(string text) => UpdateText(text, Decrypt);
        /// <summary>
        /// Распаковать все секреты в составе строки.
        /// </summary>
        /// <param name="text">Строка, в которой нужно распаковать все секреты.</param>
        /// <returns>Возвращается строка с распакованными секретами.</returns>
        public string Expand(string text) => UpdateText(text);
        /// <summary>
        /// Получить распакованное значение секрета.
        /// </summary>
        /// <param name="secret">Секрет, подлежащий распаковке.</param>
        /// <returns>Распакованное значение секрета.</returns>
        public string Expand(ISecret secret) => secret.IsEncrypted ? Decrypt(secret).Data : secret.Data;

        /// <summary>
        /// Распарсить текст в массив INode.
        /// </summary>
        /// <param name="text">Текст, подлежащий парсингу.</param>
        /// <returns>Экземпляр INodeCollection.</returns>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        /// <exception cref="ParsingException">Ошибка парсинга.</exception>
        public INodeCollection ParseToNodes(string text) {

            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            var secretLocations = new List<SecretLocation>();
            var nodes = new List<INode>();

            foreach (var tagPair in new[] { OptionsBase.EncryptedTagPair, OptionsBase.DecryptedTagPair }) {

                var tmpText = text;
                var offset = 0;

                while (true) {
                    var openTagIndex = tmpText.IndexOf(tagPair.OpenTag);
                    if (openTagIndex < 0) break;

                    var closeTagIndex = tmpText.IndexOf(tagPair.CloseTag);
                    if (closeTagIndex < 0) throw new ParsingException($"Количество закрывающихся тегов {tagPair.CloseTag} не совпадает с количеством открывающихся {tagPair.OpenTag}.");

                    var secretString = tmpText.Substring(openTagIndex, closeTagIndex - openTagIndex + tagPair.CloseTag.Length);
                    var secret = ParseSecret(secretString);
                    var secretLocation = new SecretLocation(offset + openTagIndex, offset + closeTagIndex + tagPair.CloseTag.Length, secret);
                    secretLocations.Add(secretLocation);
                    offset += closeTagIndex + tagPair.CloseTag.Length;
                    tmpText = tmpText.Substring(closeTagIndex + tagPair.CloseTag.Length);
                }
            }

            if (secretLocations.Count == 0) return new NodeCollection(new[] { new TextNode(0, text) });

            var orderedSecretLocations = secretLocations.OrderBy(n => n.StartIndex).ToArray();
            var substringOffset = 0;
            int nodeIndex = 0;

            foreach (var secretLocation in orderedSecretLocations) {

                var substring = text.Substring(substringOffset, secretLocation.StartIndex - substringOffset);

                if (!string.IsNullOrEmpty(substring)) nodes.Add(new TextNode(nodeIndex++, substring));

                nodes.Add(new SecretNode(nodeIndex++, secretLocation));

                substringOffset = secretLocation.EndIndex;
            }

            var tail = text.Substring(substringOffset);
            if (!string.IsNullOrEmpty(tail)) nodes.Add(new TextNode(nodeIndex++, tail));

            return new NodeCollection(nodes.ToArray());
        }

        /// <summary>
        /// Выполнить в тексте операцию, указанную в параметре func над каждым секретом.
        /// </summary>
        /// <param name="text">Текст, подлежащий обработке.</param>
        /// <param name="func">Операция, которую нужно выполнить над каждым секретом.</param>
        /// <returns>Строка, содержащая результат преобразований, выполненных над секретами.</returns>
        /// <exception cref="ParsingException">В строке количество тегов закрытия секретов
        /// не равно количеству тегов их открытия.</exception>
        private string UpdateText(string text, Func<ISecret, ISecret> func = null) {

            if (string.IsNullOrEmpty(text)) return text;

            var nodes = ParseToNodes(text);
            var sb = new StringBuilder();

            foreach (var node in nodes.OrderBy(n => n.Index)) {

                if (node.NodeType == NodeType.Text) {

                    var textNode = (ITextNode)node;
                    sb.Append(textNode.Value);
                }
                else if (node.NodeType == NodeType.Secret) {

                    var secretNode = (ISecretNode)node;

                    if (func == null) {
                        sb.Append(Expand(secretNode.Value.Secret));
                    }
                    else {
                        sb.Append(func(secretNode.Value.Secret));
                    }
                }
                else {
                    throw new ParsingException($"Не ожиданное значение перечисления {nameof(NodeType)}: {node.NodeType}.");
                }
            }

            return sb.ToString();
        }
    }
}
