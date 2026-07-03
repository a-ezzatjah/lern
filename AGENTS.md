# AGENTS.md

## Quick start

```powershell
dotnet restore
dotnet build
dotnet run --project lern
```

Opens Swagger UI at `http://localhost:5180/swagger` by default (see `lern/Properties/launchSettings.json`).

## Solution layout (4 projects, .NET 8)

| Project | Role | Depends on |
|---|---|---|
| `lern/` | ASP.NET Core Web API (startup) | Service, ServiceContract |
| `Service/` | Business logic, AutoMapper profiles, FluentValidation validators | Entities, ServiceContract |
| `ServiceContract/` | DTOs, service interfaces, query objects, common types | Entities |
| `Entities/` | EF Core `ShopDbContext`, entity models, migrations, enums | — |

## Architecture notes

- Traditional 4-layer: Controller → Service Interface/Impl → EF Core DbContext → SQL Server
- `IProductService` mixes sync/async (some methods return `Task<>`, others not). `ICategoryService` is fully async.
- `DbSet` property names are lowercase: `products`, `categories`, `productCategories`.
- Controller uses `[Route("api/[controller]")]` convention.
- No tests or CI workflows exist yet.
- `.github/` directory present but empty of workflows.

## Namespace quirks (trust the folder, not the namespace)

Several enum files in `Entities/Enums/` declare **misleading namespaces** — the compiler resolves by namespace, so imports must match the declared namespace, not the file location:

| File | Declared namespace |
|---|---|
| `Entities/Enums/DisconTypeEnum.cs` | `DTO` |
| `Entities/Enums/OrderEnum.cs` | `DTO` |
| `Entities/Enums/EnumProductSearchType.cs` | `ServiceContract.Enums` |
| `Entities/Enums/EnumProductSortType.cs` | `ServiceContract.Enums` |

Convention: `using DTO;` pulls in `DisconTypeEnum` and `OrderEnum`. The namespace `ServiceContract.Enums` is declared in the **Entities** project.

## Framework stack (from csproj files)

- **EF Core** 8.0.26 + SQL Server provider
- **AutoMapper** 16.1.1 — profiles in `Service/Mapping/`
- **FluentValidation** 8.6.3 — validators in `Service/Validators/`
- **Swashbuckle** 10.2.1 (Swagger)
- **MemoryCache** registered in DI (not yet used in services)
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnectionstring` (local SQL Server, Windows Auth)

## Known issues

- `ProductService.Update` validates with `IValidator<DtoproductAdd>` (`_validations`) instead of the update validator (`_updateValidator`).
- Persian/Farsi error messages are used throughout.
- `GetBranchName` on `ProductService` throws `NotImplementedException`.
