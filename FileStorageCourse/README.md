# FileStorageMVC

Простое файловое хранилище на C# (`ASP.NET MVC 5` + `Entity Framework 6` + `SQL Server LocalDB`) с:

- загрузкой файла на сервер;
- сохранением файла в `App_Data/Uploads`;
- сохранением метаданных в БД (`FileStorageDb`, таблица `Files`);
- скачиванием файла по `Id` через контроллер.

## Что реализовано

### Модель и БД

- `Models/FileRecord.cs` - модель файла (`OriginalFileName`, `SavedFileName`, `ContentType`, `Size`, `UploadDate`)
- `Models/AppDbContext.cs` - EF-контекст (`DbSet<FileRecord> Files`)
- `Migrations/*` - миграции для создания структуры БД

### Репозиторий

- `Repositories/IFileRepository.cs` - контракт доступа к данным
- `Repositories/FileRepository.cs` - реализация добавления и поиска файла по `Id`

### Контроллер и представления

- `Controllers/FileController.cs`
  - `GET /File/UploadFile` - форма загрузки
  - `POST /File/UploadFile` - загрузка, запись файла и метаданных
  - `GET /File/DownloadFile/{id}` - скачивание файла
- `Views/File/UploadFile.cshtml` - страница загрузки
- `Views/Shared/_Layout.cshtml` - ссылки в меню навигации

## Конфигурация

Настройки находятся в `Web.config`.

### Connection string

```xml
<connectionStrings>
  <add name="DefaultConnection"
       providerName="System.Data.SqlClient"
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=FileStorageDb;Integrated Security=True" />
</connectionStrings>
```

### AppSettings

```xml
<appSettings>
  <add key="UploadFolder" value="~/App_Data/Uploads" />
  <add key="MaxFileSizeBytes" value="10485760" />
</appSettings>
```

### Ограничения ASP.NET

```xml
<httpRuntime targetFramework="4.7.2" maxRequestLength="30720" executionTimeout="120" />
```

## Как запустить (Visual Studio 2022)

## 1) Открыть решение

Откройте файл:

`FileStorageMVC/FileStorageMVC.sln`

## 2) Восстановить NuGet-пакеты

Обычно пакеты восстанавливаются автоматически при открытии решения.  
При необходимости: `Build -> Restore NuGet Packages`.

## 3) Применить миграции (Package Manager Console)

Откройте: `Инструменты -> Диспетчер пакетов NuGet -> Консоль диспетчера пакетов`

Убедитесь, что в "Проект по умолчанию" выбран `FileStorageMVC`, затем выполните:

```powershell
Enable-Migrations
Add-Migration InitialCreate
Update-Database
```

Если миграции уже созданы, обычно достаточно:

```powershell
Update-Database
```

## 4) Запуск

Нажмите `F5` в Visual Studio.

После старта откройте:

- страница загрузки: `/File/UploadFile`
- скачивание по id: `/File/DownloadFile/{id}`

Пример:

`/File/DownloadFile/1`

## Быстрая проверка

1. Перейти на `/File/UploadFile`.
2. Загрузить файл до `10 MB`.
3. Убедиться, что отображается сообщение вида: `Файл загружен. Id = X`.
4. Открыть `/File/DownloadFile/X` и проверить скачивание.

## Структура проекта

- `FileStorageMVC/FileStorageMVC/Controllers`
- `FileStorageMVC/FileStorageMVC/Models`
- `FileStorageMVC/FileStorageMVC/Repositories`
- `FileStorageMVC/FileStorageMVC/Views`
- `FileStorageMVC/FileStorageMVC/Migrations`
