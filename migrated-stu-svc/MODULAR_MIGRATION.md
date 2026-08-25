# ct.stu - .NET 10 / C# 14 / PostgreSQL modular migration

## Target architecture

The application is organized by module and responsibility:

- `Modules/Students/Api` - HTTP endpoints.
- `Modules/Students/Application` - student use cases/services.
- `Modules/Students/Domain` - student resources/domain types.
- `Modules/Students/Contracts` - student DTOs/contracts.
- `Modules/Students/Infrastructure/Queries` - PostgreSQL read queries.
- `Modules/Students/Infrastructure/Commands` - PostgreSQL write commands.
- `Modules/Students/Infrastructure/Database` - indexes/database-specific artifacts.
- `Modules/Reports/*` - report API/application code.
- `Modules/Shared/*` - shared contracts.
- `Infrastructure/Database` - PostgreSQL persistence implementation.
- `Database/` - schema, views, indexes and versioned SQL queries.

## Important migration rule

New code must not introduce RavenDB APIs. Database access belongs in the module infrastructure layer. Controllers must not contain SQL.

The legacy-shaped `StudentQry` / `StudentCmd` implementations are retained as a compatibility seam while individual operations are moved to the PostgreSQL-native `StudentQueries` / `StudentCommands` classes. This allows endpoint-by-endpoint replacement without changing public API contracts all at once.

## PostgreSQL-native Student query example

`Modules/Students/Infrastructure/Queries/StudentQueries.cs` uses `NpgsqlDataSource` and executes parameterized SQL directly against PostgreSQL. It is not an in-memory emulation of RavenDB query semantics.

## Database

`Database/schema.sql` contains the PostgreSQL schema, indexes and views. `Database/queries/students.sql` contains version-controlled Student query text.

## Verification

The generation environment does not contain the .NET SDK, so `dotnet restore`, `dotnet build`, database integration tests and endpoint tests must be run in the target CI/development environment.
