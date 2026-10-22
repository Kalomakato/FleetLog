# FleetLog

FleetLog is an ASP.NET Core MVC web application for managing company vehicles, intended for municipalities and enterprises in Bulgaria.

It keeps vehicle details, mileage, refuelling and repair histories in one place. The interface is in Bulgarian, distances are recorded in kilometres, and fuel quantities are recorded in litres.

## Implemented features

### Vehicles

- View the vehicle list and individual vehicle details.
- Add and edit vehicles.
- Store registration number, make, model, year, VIN, fuel type, mileage and active status.
- Validate vehicle data and prevent duplicate registration numbers and VINs.
- Delete vehicles only when they have no associated fuel entries or repairs.

### Fuel entries

- View refuelling history for each vehicle.
- Add entries containing date, litres, total cost and mileage.
- Display the newest entries first.
- Update the vehicle's current mileage when an entry is saved.
- Validate required fields and numerical limits.
- Reject future dates and mileage below the vehicle's current mileage.

### Repairs

- View repair history for each vehicle.
- Add repairs containing date, description, total cost, mileage and an optional service centre.
- Display the newest repairs first.
- Update the vehicle's current mileage when a repair is saved.
- Validate required fields, text lengths and numerical limits.
- Reject future dates and mileage below the vehicle's current mileage.

Fuel and repair entries currently support viewing and adding. Editing and deleting these entries are outside the current scope.

### Interface

- Responsive layouts for desktop and narrow screens.
- Consistent navigation, forms, panels and action buttons.
- Empty-history messages.
- Bulgarian validation messages.
- Scrollable tables on smaller screens.

## Technologies

- C# and .NET 8
- ASP.NET Core MVC
- Razor views and Tag Helpers
- Entity Framework Core 8.0.31
- Microsoft SQL Server
- Bootstrap, HTML and CSS
- jQuery Validation and Unobtrusive Validation

## Prerequisites

- .NET 8 SDK
- Git
- A running Microsoft SQL Server instance accessible from your computer
- Entity Framework Core CLI tools, version 8.0.31

SQL Server may run locally, on another server, or in a configured Docker container. Start the database server before applying migrations or running FleetLog.

The repository does not provision SQL Server automatically.

## Local setup

Run the following commands in a terminal.

### 1. Clone the repository

```bash
git clone https://github.com/Kalomakato/FleetLog.git
cd FleetLog
```

All remaining commands assume you are in this repository folder, which contains the nested `FleetLog` project folder.

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Install the EF Core command-line tool

If the tool is not already installed:

```bash
dotnet tool install --global dotnet-ef --version 8.0.31
```

If it is already installed and needs updating to this version:

```bash
dotnet tool update --global dotnet-ef --version 8.0.31
```

Check the installed version:

```bash
dotnet ef --version
```

### 4. Configure the database connection

FleetLog expects a connection string named `DefaultConnection`.

The project already has a User Secrets ID configured. For local development, store your connection string using User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=localhost,1433;Database=FleetLog;User Id=YOUR_SQL_USER;Password=YOUR_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True;' --project FleetLog/FleetLog.csproj
```

Replace the server, port, database name and credentials with values appropriate to your SQL Server instance.

This example uses SQL Server authentication. `TrustServerCertificate=True` is intended for a local development server with a self-signed certificate. Use an appropriately configured certificate for deployment.

Do not commit real passwords or connection strings containing credentials to Git.

### 5. Apply database migrations

The repository includes the `InitialCreate` migration. Apply it to create the application's database schema.

On macOS/Linux:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --project FleetLog/FleetLog.csproj --startup-project FleetLog/FleetLog.csproj
```

On Windows PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project FleetLog/FleetLog.csproj --startup-project FleetLog/FleetLog.csproj
```

Setting the environment to `Development` allows the application configuration to load local User Secrets.

The configured SQL Server account must have the permissions required to create or update the database.

Existing migrations should be applied when setting up the project; creating a new migration is not required.

### 6. Build and run

```bash
dotnet build
dotnet run --project FleetLog/FleetLog.csproj --launch-profile http
```

Open:

http://localhost:5185

To use the HTTPS profile, first trust the development certificate:

```bash
dotnet dev-certs https --trust
dotnet run --project FleetLog/FleetLog.csproj --launch-profile https
```

Then open:

https://localhost:7103

## Environment settings

| Setting | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Selects the application environment. The supplied HTTP and HTTPS launch profiles use `Development`. |
| `ConnectionStrings:DefaultConnection` | Database connection string used by Entity Framework Core. |
| `ConnectionStrings__DefaultConnection` | Environment-variable equivalent of the database connection setting. |

User Secrets are used for local development. A deployed environment must supply its own database configuration.

Launch profiles are stored in `FleetLog/Properties/launchSettings.json`.

Migrations are applied explicitly using `dotnet ef database update`; application startup does not automatically apply them.

## Project structure

```text
FleetLog/
├── Controllers/          MVC request handling
├── Data/
│   ├── Enums/            Fuel type definitions
│   ├── Migrations/       EF Core database migrations
│   └── Models/           Database entities
├── ViewModels/           View data and input validation
├── Views/                Razor pages and shared layouts
├── wwwroot/              CSS, JavaScript and static libraries
└── Program.cs            Application and service configuration
```

## Manual verification

The following flows have been checked during development:

- Vehicle navigation and details.
- Adding fuel entries and repairs.
- Required-field, range, date and mileage validation.
- Updating vehicle mileage after saving an entry.
- Keeping histories associated with the correct vehicle.
- Empty fuel and repair histories.
- Preventing deletion of vehicles with related records.
- Navigation and layouts at wide and narrow screen sizes.

A fresh-clone setup check using these README instructions remains to be completed.

## Current limitations

- Fuel and repair entries cannot yet be edited or deleted.
- Historical entries with mileage below the vehicle's current mileage are rejected.
- Authentication and role-based access control are not implemented. The application is currently intended as a local educational project.

## Български — Пълна документация

FleetLog е ASP.NET Core MVC уеб приложение за управление на служебни
автомобили, предназначено за общини и предприятия в България.

То събира на едно място данните за автомобилите, километража,
зарежданията, ремонтите и свързаните разходи. Интерфейсът е на
български език. Разстоянията се записват в километри, а количеството
гориво — в литри.

### Реализирани функционалности

#### Автомобили

- Преглед на списъка с автомобили и подробностите за всеки автомобил.
- Добавяне и редактиране на автомобили.
- Съхраняване на регистрационен номер, марка, модел, година, VIN,
  вид гориво, километраж и активен статус.
- Проверка на въведените данни.
- Предотвратяване на дублирани регистрационни номера и VIN номера.
- Изтриване на автомобил само когато няма свързани зареждания или ремонти.

#### Зареждания

- Преглед на историята на зарежданията за конкретен автомобил.
- Добавяне на дата, количество гориво, обща сума и километраж.
- Показване на най-новите записи първо.
- Обновяване на текущия километраж на автомобила при записване.
- Проверка на задължителните полета и допустимите числови стойности.
- Отхвърляне на бъдещи дати и километраж под текущия за автомобила.

#### Ремонти

- Преглед на историята на ремонтите за конкретен автомобил.
- Добавяне на дата, описание, обща сума, километраж и незадължително
  посочване на сервиз.
- Показване на най-новите записи първо.
- Обновяване на текущия километраж на автомобила при записване.
- Проверка на задължителните полета, дължината на текста и числовите стойности.
- Отхвърляне на бъдещи дати и километраж под текущия за автомобила.

Зарежданията и ремонтите могат да се преглеждат и добавят.
Редактирането и изтриването на тези записи не са включени в текущия обхват.

#### Интерфейс

- Адаптивно оформление за широки и тесни екрани.
- Единен дизайн на навигацията, формите, панелите и бутоните.
- Съобщения при липса на записи в историята.
- Съобщения за валидация на български език.
- Таблици с хоризонтално превъртане при по-малки екрани.

### Използвани технологии

- C# и .NET 8
- ASP.NET Core MVC
- Razor изгледи и Tag Helpers
- Entity Framework Core 8.0.31
- Microsoft SQL Server
- Bootstrap, HTML и CSS
- jQuery Validation и Unobtrusive Validation

### Предварителни изисквания

- .NET 8 SDK
- Git
- Работещ Microsoft SQL Server, достъпен от вашия компютър
- Entity Framework Core CLI, версия 8.0.31

SQL Server може да работи локално, на отделен сървър или в конфигуриран
Docker контейнер. Стартирайте сървъра за базата данни преди прилагане
на миграциите или стартиране на FleetLog.

Хранилището не инсталира и не конфигурира SQL Server автоматично.

### Локално инсталиране и стартиране

Изпълнете следните команди в терминал.

#### 1. Клониране на хранилището

```bash
git clone https://github.com/Kalomakato/FleetLog.git
cd FleetLog
```

Всички следващи команди се изпълняват от основната папка на хранилището,
която съдържа вложената папка на проекта `FleetLog`.

#### 2. Възстановяване на зависимостите

```bash
dotnet restore
```

#### 3. Инсталиране на инструмента EF Core CLI

Ако инструментът все още не е инсталиран:

```bash
dotnet tool install --global dotnet-ef --version 8.0.31
```

Ако вече е инсталиран и трябва да бъде обновен до тази версия:

```bash
dotnet tool update --global dotnet-ef --version 8.0.31
```

Проверете инсталираната версия:

```bash
dotnet ef --version
```

#### 4. Настройване на връзката с базата данни

FleetLog използва низ за връзка с име `DefaultConnection`.

Проектът вече има конфигуриран идентификатор за User Secrets.
За локална разработка запишете низа за връзка чрез:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=localhost,1433;Database=FleetLog;User Id=YOUR_SQL_USER;Password=YOUR_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True;' --project FleetLog/FleetLog.csproj
```

Заменете сървъра, порта, името на базата данни и данните за достъп
с подходящите стойности за вашия SQL Server.

Примерът използва удостоверяване с потребителско име и парола за SQL Server.
`TrustServerCertificate=True` е предназначено за локална разработка
със самоподписан сертификат. При внедряване използвайте правилно
конфигуриран сертификат.

Не добавяйте реални пароли или низове за връзка с данни за достъп в Git.

#### 5. Прилагане на миграциите

Хранилището включва миграцията `InitialCreate`.
Приложете я, за да създадете структурата на базата данни.

За macOS/Linux:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --project FleetLog/FleetLog.csproj --startup-project FleetLog/FleetLog.csproj
```

За Windows PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project FleetLog/FleetLog.csproj --startup-project FleetLog/FleetLog.csproj
```

Средата `Development` позволява зареждането на локалните User Secrets.

Потребителят на SQL Server трябва да има необходимите права за
създаване или обновяване на базата данни.

При първоначално настройване се прилагат съществуващите миграции.
Не е необходимо да създавате нова миграция.

#### 6. Компилиране и стартиране

```bash
dotnet build
dotnet run --project FleetLog/FleetLog.csproj --launch-profile http
```

Отворете:

http://localhost:5185

За HTTPS първо добавете сертификата за разработка към доверените сертификати:

```bash
dotnet dev-certs https --trust
dotnet run --project FleetLog/FleetLog.csproj --launch-profile https
```

След това отворете:

https://localhost:7103

### Настройки на средата

| Настройка | Предназначение |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Определя средата на приложението. Предоставените HTTP и HTTPS профили използват `Development`. |
| `ConnectionStrings:DefaultConnection` | Низът за връзка с базата данни, използван от Entity Framework Core. |
| `ConnectionStrings__DefaultConnection` | Еквивалентната настройка чрез променлива на средата. |

User Secrets се използват за локална разработка.
При внедряване трябва да се предостави отделна конфигурация
за връзката с базата данни.

Профилите за стартиране се намират във
`FleetLog/Properties/launchSettings.json`.

Миграциите се прилагат изрично чрез `dotnet ef database update`.
Стартирането на приложението не ги прилага автоматично.

### Структура на проекта

```text
FleetLog/
├── Controllers/          Обработка на MVC заявки
├── Data/
│   ├── Enums/            Дефиниции на видовете гориво
│   ├── Migrations/       EF Core миграции
│   └── Models/           Модели на базата данни
├── ViewModels/           Данни за изгледите и валидация
├── Views/                Razor изгледи и споделени оформления
├── wwwroot/              CSS, JavaScript и статични библиотеки
└── Program.cs            Конфигурация на приложението и услугите
```

### Ръчни проверки

По време на разработката са проверени:

- Навигацията между автомобилите и техните детайли.
- Добавянето на зареждания и ремонти.
- Валидацията на задължителни полета, стойности, дати и километраж.
- Обновяването на километража след записване.
- Свързването на историята с правилния автомобил.
- Празните истории на зарежданията и ремонтите.
- Забраната за изтриване на автомобили със свързани записи.
- Навигацията и оформлението при широки и тесни екрани.

Предстои проверка на инсталирането от ново клониране на хранилището,
следвайки инструкциите в този README.

### Текущи ограничения

- Зарежданията и ремонтите все още не могат да се редактират или изтриват.
- По-стари записи с километраж под текущия за автомобила се отхвърлят.
- Не са реализирани потребителски профили и контрол на достъпа чрез роли.
  Приложението в момента е предназначено за локален учебен проект.