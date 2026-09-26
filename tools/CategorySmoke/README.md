Run from the repository root:

```powershell
dotnet run --project tools/CategorySmoke
```

Uses the database configured in `lern/appsettings.json` for read-only integration checks. It does not run migrations, seed, or change data. Checks category filtering (including descendants), ordering, page bounds, controller models, and unknown-category 404 responses against existing records. Multi-page coverage depends on available product counts.
