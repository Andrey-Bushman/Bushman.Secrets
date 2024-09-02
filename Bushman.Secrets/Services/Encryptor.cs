using Bushman.Secrets.Abstractions.Exceptions;
using Bushman.Secrets.Abstractions.Models;
using Bushman.Secrets.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

// https://learn.microsoft.com/ru-ru/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-8.0

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
            var minExpectedCount = 3;

            if (secretInfoValues.Length < minExpectedCount) throw new ParsingException(
                $"Ожидаемое количество свойств в секрете: {minExpectedCount}. Фактическое: {secretInfoValues.Length}.");

            var options = new SecretOptions(
                OptionsBase,
                (StoreLocation)Enum.Parse(typeof(StoreLocation),
                secretInfoValues[0].Trim()),
                secretInfoValues[1].Trim()
            );

            var secretData = string.Join($"{OptionsBase.FieldSeparator}", secretInfoValues.Skip(2));

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

            X509Certificate2 certificate = null;
            ISecret result = null;

            using (X509Store store = new X509Store(secret.Options.StoreLocation)) {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, secret.Options.Thumbprint, false);

                if (certificates.Count > 0) {
                    certificate = certificates[0];
                }

                if (certificate == null) {

                    store.Close();

                    throw new CryptographicException($"Сертификат {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" не найден в хранилище {secret.Options.StoreLocation}.");
                }

                Aes aes = Aes.Create();
                aes.KeySize = secret.Options.OptionsBase.AesKeySize;
                aes.Mode = secret.Options.OptionsBase.AesCipherMode;

                using (certificate)
                using (aes)
                using (ICryptoTransform transform = aes.CreateEncryptor()) {

                    RSAPKCS1KeyExchangeFormatter keyFormatter = new RSAPKCS1KeyExchangeFormatter(certificate.GetRSAPublicKey());
                    byte[] keyEncrypted = keyFormatter.CreateKeyExchange(aes.Key, aes.GetType());

                    byte[] LenK = new byte[sizeof(int)];
                    byte[] LenIV = new byte[sizeof(int)];

                    LenK = BitConverter.GetBytes(keyEncrypted.Length);
                    LenIV = BitConverter.GetBytes(aes.IV.Length);

                    using (MemoryStream outputStream = new MemoryStream()) {
                        outputStream.Write(LenK, 0, LenK.Length);
                        outputStream.Write(LenIV, 0, LenIV.Length);
                        outputStream.Write(keyEncrypted, 0, keyEncrypted.Length);
                        outputStream.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream outStreamEncrypted = new CryptoStream(outputStream, transform, CryptoStreamMode.Write)) {
                            int count = 0;
                            byte[] data = new byte[aes.BlockSize / 8];
                            int bytesRead = 0;

                            using (MemoryStream inputStream = new MemoryStream()) {

                                using (var sw = new StreamWriter(inputStream, secret.Options.OptionsBase.Encoding, 1024, true)) {
                                    sw.Write(secret.Data);
                                    sw.Flush();
                                }
                                inputStream.Seek(0, SeekOrigin.Begin);

                                do {
                                    count = inputStream.Read(data, 0, data.Length);
                                    outStreamEncrypted.Write(data, 0, count);
                                    bytesRead += count;
                                } while (count > 0);

                                inputStream.Close();
                            }
                            outStreamEncrypted.FlushFinalBlock();
                            outputStream.Flush();

                            outputStream.Seek(0, SeekOrigin.Begin);
                            string encryptedData = Convert.ToBase64String(outputStream.ToArray());
                            result = new Secret(secret.Options, encryptedData, true);
                            outStreamEncrypted.Close();
                        }
                        outputStream.Close();
                    }
                }
                store.Close();
            }
            return result;
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
                return new Secret(secret.Options, secret.Data, true);
            }

            X509Certificate2 certificate = null;
            ISecret result = null;

            using (X509Store store = new X509Store(secret.Options.StoreLocation)) {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, secret.Options.Thumbprint, false);

                if (certificates.Count > 0) {
                    certificate = certificates[0];
                }

                if (certificate == null) {

                    store.Close();

                    throw new CryptographicException($"Сертификат {nameof(X509Certificate2)} с отпечатком \"{secret.Options.Thumbprint}\" не найден в хранилище {secret.Options.StoreLocation}.");
                }

                Aes aes = Aes.Create();
                aes.KeySize = secret.Options.OptionsBase.AesKeySize;
                aes.Mode = secret.Options.OptionsBase.AesCipherMode;

                byte[] LenK = new byte[sizeof(int)];
                byte[] LenIV = new byte[sizeof(int)];

                using (MemoryStream inputStream = new MemoryStream()) {
                    var encryptedBytes = Convert.FromBase64String(secret.Data);
                    inputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    inputStream.Flush();

                    inputStream.Seek(0, SeekOrigin.Begin);
                    inputStream.Read(LenK, 0, LenK.Length);
                    inputStream.Read(LenIV, 0, LenIV.Length);

                    int lenK = BitConverter.ToInt32(LenK, 0);
                    int lenIV = BitConverter.ToInt32(LenIV, 0);

                    byte[] KeyEncrypted = new byte[lenK];
                    byte[] IV = new byte[lenIV];

                    inputStream.Read(KeyEncrypted, 0, lenK);
                    inputStream.Read(IV, 0, lenIV);

                    byte[] KeyDecrypted = certificate.GetRSAPrivateKey().Decrypt(KeyEncrypted, RSAEncryptionPadding.Pkcs1);

                    using (ICryptoTransform transform = aes.CreateDecryptor(KeyDecrypted, IV)) {

                        using (MemoryStream outputStream = new MemoryStream()) {
                            int count = 0;                            
                            byte[] data = new byte[aes.BlockSize / 8];

                            using (CryptoStream outStreamEncrypted = new CryptoStream(outputStream, transform, CryptoStreamMode.Write)) {
                                do {
                                    count = inputStream.Read(data, 0, data.Length);
                                    outStreamEncrypted.Write(data, 0, count);
                                } while (count > 0);

                                if (outStreamEncrypted.HasFlushedFinalBlock) outStreamEncrypted.FlushFinalBlock();

                                outputStream.Seek(0, SeekOrigin.Begin);
                                string decryptedData = secret.Options.OptionsBase.Encoding.GetString(outputStream.ToArray());
                                result = new Secret(secret.Options, decryptedData, false);
                                outStreamEncrypted.Close();
                            }
                        }
                    }
                    inputStream.Close();
                }
            }
            return result;
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
