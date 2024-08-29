
# secret (Bushman.Secrets.CLI)

Консоль для шифрования, расшифрования и распаковки секретов в текстовых файлах.
Используется модель секретов, определённая в пакете [Bushman.Secrets.Abstractions](https://www.nuget.org/packages/Bushman.Secrets.Abstractions/).

Порядок передачи параметров при запуске приложения не имеет значения.

В конфигурационном файле приложения хранятся настройки, используемые по умолчанию:

  * `encoding` - кодировка обрабатываемых текстовых файлов. Если кодировка целевого
	файла отличается от указанной в этом параметре, то укажите соответствующее значение
	одноимённым параметром, при запуске приложения.

## Операции

Для файла, указанного через параметр `file`, может быть выполнена одна из следующих операций:
  
  * `encrypt` - зашифровать все расшифрованные секреты.
  * `decrypt` - расшифровать все зашифрованные секреты.
  * `expand` - распаковать все секреты (т.е. заменить все секреты на хранящиеся в них расшифрованные значения).

Схема запуска приложения:

```
dotnet secret.dll --file FileName --operation <encrypt|decrypt|expand> [--encoding EncodingName]
```

Примеры:

```
dotnet secret.dll --file .\appsettings.json --operation encrypt

dotnet secret.dll --file .\appsettings.json --operation decrypt --encoding utf-8

dotnet secret.dll --file .\appsettings.json --operation expand
```
