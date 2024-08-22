
namespace Bushman.Secrets.Abstractions.Services {
    /// <summary>
    /// Провайдер фабрики секретов.
    /// </summary>
    public interface ISecretFactoryProvider {
        /// <summary>
        /// Получить экземпляр публичного класса, реализующего интерфейс ISecretFactory, по имени нужного вам провайдера.
        /// В классе должен быть определён публичный конструктор по умолчанию.
        /// </summary>
        /// <param name="providerName">Имя провайдера (это имя сборки, в составе которой определена реализация интенфейса ISecretFactory).</param>
        /// <returns>Экземпляр ISecretFactory.</returns>
        ISecretFactory CreateSecretFactory(string providerName);

        /// <summary>
        /// Получить экземпляр публичного класса, реализующего интерфейс ISecretFactory, по имени нужного вам провайдера
        /// и по полному имени этого класса (с указанием пространства имён). В классе должен быть определён публичный конструктор по умолчанию.
        /// </summary>
        /// <param name="providerName">Имя провайдера (это имя сборки, в составе которой определена реализация интенфейса ISecretFactory).</param>
        /// <param name="className">Полное имя публичного класса (с указанием пространства имён), реализующего интерфейс ISecretFactory.</param>
        /// <returns>Экземпляр ISecretFactory.</returns>
        ISecretFactory CreateSecretFactory(string providerName, string className);
    }
}
