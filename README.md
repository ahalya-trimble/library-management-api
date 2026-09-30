# Library Management API

This is an ASP.NET Core API for a library. It manages books, categories, members, loans, reviews, and users.

## What This Project Uses

- .NET 10 and ASP.NET Core
- SQL Server with Entity Framework Core
- JWT login and ASP.NET Core Identity
- Swagger for API testing
- FluentValidation and AutoMapper
- xUnit tests with Moq and EF Core InMemory

## Project Folders

- `LibraryManagementAPI/` - The API project
- `LibraryManagementAPI.Tests/` - The test project
- `LibraryManagementAPI.slnx` - The solution file

## Requirements

- .NET SDK 10
- SQL Server or SQL Server Express
- Permission to create and update the application database

## Configure the Application

Edit `LibraryManagementAPI/appsettings.json` for local use. Set the SQL Server connection string and a random JWT key with at least 32 characters.

Do not commit real passwords or production JWT keys. Use user secrets or environment variables for private values.

## Run the API

Run these commands from the repository folder:

```powershell
dotnet restore .\LibraryManagementAPI.slnx
dotnet run --project .\LibraryManagementAPI\LibraryManagementAPI.csproj
```

In Development, open Swagger at:

- `https://localhost:7052/swagger`
- `http://localhost:5073/swagger`

The API applies pending database migrations when it starts. The SQL Server account must be able to create or update the database.

## Run the Tests

```powershell
dotnet test .\LibraryManagementAPI.Tests\LibraryManagementAPI.Tests.csproj
```

## API Routes

| Area | Route |
| --- | --- |
| Authentication | `/api/auth` |
| Books | `/api/books` |
| Categories | `/api/categories` |
| Loans | `/api/loans` |
| Members | `/api/members` |
| Reviews | `/api/reviews` |

Example requests are in `LibraryManagementAPI/LibraryManagementAPI.http`.

## Entity Framework Core Migrations

Migrations are stored in `LibraryManagementAPI/Migrations`.

### Visual Studio Package Manager Console

Open **Tools > NuGet Package Manager > Package Manager Console** in Visual Studio. Set **Default project** to `LibraryManagementAPI`, then run:

```powershell
# Create a migration after changing a model
Add-Migration MigrationName -Project LibraryManagementAPI -StartupProject LibraryManagementAPI

# Apply migrations to the database
Update-Database -Project LibraryManagementAPI -StartupProject LibraryManagementAPI
```

Replace `MigrationName` with a name that describes the change, such as `AddBookDescription`.

### .NET CLI

```powershell
# Create a migration
dotnet ef migrations add MigrationName --project .\LibraryManagementAPI\LibraryManagementAPI.csproj --startup-project .\LibraryManagementAPI\LibraryManagementAPI.csproj

# Apply migrations
dotnet ef database update --project .\LibraryManagementAPI\LibraryManagementAPI.csproj --startup-project .\LibraryManagementAPI\LibraryManagementAPI.csproj
```
