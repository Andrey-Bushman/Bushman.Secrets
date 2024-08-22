using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bushman.Secrets.Abstractions.Services {
    /// <summary>
    /// Провайдер фабрики секретов.
    /// </summary>
    public sealed class SecretFactoryProvider : ISecretFactoryProvider {
        /// <summary>
        /// Получить экземпляр публичного класса, реализующего интерфейс ISecretFactory, по имени нужного вам провайдера.
        /// В классе должен быть определён публичный конструктор по умолчанию.
        /// </summary>
        /// <param name="providerName">Имя провайдера (это имя сборки, в составе которой определена реализация интенфейса ISecretFactory).</param>
        /// <returns>Экземпляр ISecretFactory.</returns>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public ISecretFactory CreateSecretFactory(string providerName) {

            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));

            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(n => n.GetName().Name.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));

            var dllFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{providerName}.dll");
            var exeFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{providerName}.exe");

            if (asm == null && File.Exists(dllFileName)) asm = Assembly.LoadFile(dllFileName);
            else if (asm == null && File.Exists(exeFileName)) asm = Assembly.LoadFile(exeFileName);

            if (asm == null) throw new ArgumentException($"Не удалось найти сборку \"{dllFileName}\" или \"{exeFileName}\".");

            var types = asm.GetTypes().Where(n => n.IsClass && n.IsPublic && n.GetInterfaces().Contains(typeof(ISecretFactory))).ToArray();

            if (types.Length > 1) throw new ArgumentException($"Нет однозначности: в сборке \"{asm.Location}\" найдено несколько публичных классов, "
                + $"реализующих интерфейс {typeof(ISecretFactory).FullName}: {string.Join(", ", types.Select(n => n.FullName))}. Чтобы явным образом "
                + $"указать конкретный класс, используйте другую перегруженную версию метода {nameof(CreateSecretFactory)}.");

            if (types.Length == 0) throw new ArgumentException($"В сборке \"{asm.Location}\" не удалось найти публичный класс, реализующий интерфейс {typeof(ISecretFactory).FullName}.");

            var factory = (ISecretFactory)Activator.CreateInstance(types[0]);
            return factory;
        }
        /// <summary>
        /// Получить экземпляр публичного класса, реализующего интерфейс ISecretFactory, по имени нужного вам провайдера
        /// и по полному имени этого класса (с указанием пространства имён). В классе должен быть определён публичный конструктор по умолчанию.
        /// </summary>
        /// <param name="providerName">Имя провайдера (это имя сборки, в составе которой определена реализация интенфейса ISecretFactory).</param>
        /// <param name="className">Полное имя публичного класса (с указанием пространства имён), реализующего интерфейс ISecretFactory.</param>
        /// <returns>Экземпляр ISecretFactory.</returns>
        /// <exception cref="ArgumentNullException">В качестве параметра передан null.</exception>
        public ISecretFactory CreateSecretFactory(string providerName, string className) {

            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrWhiteSpace(className)) throw new ArgumentNullException(nameof(className));

            var dllFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{providerName}.dll");
            var exeFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{providerName}.exe");

            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(n => n.GetName().Name.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));

            if (asm == null && File.Exists(dllFileName)) asm = Assembly.LoadFile(dllFileName);
            else if (asm == null && File.Exists(exeFileName)) asm = Assembly.LoadFile(exeFileName);

            if (asm == null) throw new ArgumentException($"Не удалось найти сборку \"{dllFileName}\" или \"{exeFileName}\".");

            var type = asm.GetTypes().Where(n => n.IsClass && n.IsPublic && n.GetInterfaces().Contains(typeof(ISecretFactory))
            && n.FullName.Equals(className, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (type == null) throw new ArgumentException($"В сборке \"{asm.Location}\" не удалось найти публичный класс {className}, реализующий интерфейс {typeof(ISecretFactory).FullName}.");

            var factory = (ISecretFactory)Activator.CreateInstance(type);
            return factory;
        }
    }
}
