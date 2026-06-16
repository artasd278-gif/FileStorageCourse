# FileStorageCourse

Учебный проект по ASP.NET MVC: мини‑файловое хранилище с загрузкой и скачиванием файлов по `Id`.

## О проекте

Это веб-приложение на `ASP.NET MVC 5` и `.NET Framework 4.7.2`, в котором:

- файл загружается через форму;
- физический файл сохраняется в `App_Data/Uploads`;
- метаданные сохраняются в БД `FileStorageDb` (LocalDB);
- файл можно скачать по адресу вида `/File/DownloadFile/{id}`.

## Технологии

- `ASP.NET MVC 5`
- `Entity Framework 6`
- `SQL Server LocalDB`
- `Bootstrap` + Razor Views
- `Git`

## Основной функционал

### 1) Загрузка файла

- Страница: `/File/UploadFile`
- При успешной загрузке показывается сообщение: `Файл загружен. Id = X`
- Ограничение размера берётся из `Web.config` (`MaxFileSizeBytes`)

### 2) Скачивание файла

- Маршрут: `/File/DownloadFile/{id}`
- Если запись отсутствует в БД -> `404 Файл не найден`
- Если запись есть, но файла на диске нет -> `404 Физический файл отсутствует`

## Архитектура

- `Models/FileRecord.cs` — модель записи о файле
- `Models/AppDbContext.cs` — EF контекст
- `Repositories/IFileRepository.cs` — контракт репозитория
- `Repositories/FileRepository.cs` — работа с таблицей `Files`
- `Controllers/FileController.cs` — upload/download логика
- `Views/File/UploadFile.cshtml` — форма загрузки

## Конфигурация (`Web.config`)

### Строка подключения

```xml
<connectionStrings>
  <add name="DefaultConnection"
       providerName="System.Data.SqlClient"
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FileStorageDb;Integrated Security=True" />
</connectionStrings>
```

### Параметры загрузки

```xml
<appSettings>
  <add key="UploadFolder" value="~/App_Data/Uploads" />
  <add key="MaxFileSizeBytes" value="10485760" />
</appSettings>
```

```xml
<httpRuntime targetFramework="4.7.2" maxRequestLength="30720" executionTimeout="120" />
```

## Запуск в Visual Studio 2022

1. Открыть решение: `FileStorageCourse/FileStorageMVC/FileStorageMVC.sln`
2. Восстановить NuGet-пакеты (если потребуется)
3. Открыть **Package Manager Console** и выполнить:

```powershell
Update-Database
```

4. Запустить проект (`F5`)
5. Проверить:
   - `/File/UploadFile`
   - `/File/DownloadFile/1`

## Статус

Проект реализован в рамках учебной практики и покрывает базовый сценарий file upload/download + хранение метаданных в БД.
