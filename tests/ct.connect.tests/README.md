# Ct.Connect Tests

MSTest suite for the **Ct.Connect** module using **EF Core + Postgres (Npgsql)**.

## Prerequisites

1. A reachable Postgres instance.
2. A dedicated database (recommended: `ct_modular_test`).
3. Connection string in one of:

   - `appsettings.json` → `ConnectionStrings:Postgres`
   - Environment variable `CT_TEST_POSTGRES`
   - Environment variable `ConnectionStrings__Postgres`
   - User secrets (`UserSecretsId`: `ct-connect-tests`)

Default:

```
Host=localhost;Port=5432;Database=ct_modular_test;Username=ct;Password=ct_dev_password
```

Create the DB once:

```bash
createdb -U ct ct_modular_test
# or
psql -U postgres -c "CREATE DATABASE ct_modular_test OWNER ct;"
```

On first test run, `EnsureCreated()` creates the Connect tables (`topics`, `sms`, `sms_messages`, `emails`).
If you already manage schema via migrations, apply migrations to the test DB instead of relying on EnsureCreated.

## Isolation

- Each test uses a **fresh `InstId` (OwnerId)** GUID so rows never collide.
- Tests call `CleanupAsync(db, user.InstId)` to delete their rows.
- RabbitMQ is disabled; jobs are published to `LoggingConnectEventPublisher`.

## Run

```bash
dotnet test path/to/ct.connect.tests.csproj
```

Override connection for CI:

```bash
export CT_TEST_POSTGRES="Host=postgres;Port=5432;Database=ct_modular_test;Username=ct;Password=secret"
dotnet test
```

## Layout

```
ct.connect.tests/
  TestBase.cs              # Postgres DbContext factory, schema ensure, cleanup
  TopicTests.cs
  SMSTests.cs
  SMSTemplateTests.cs
  ConnectTests.cs
  ReportTests.cs
  WorkflowTests.cs
  FacetTests.cs
  appsettings.json
  http/                    # manual API samples
```

## Project reference

Adjust if needed:

```xml
<ProjectReference Include="..\..\src\Modules\Ct.Connect\Ct.Connect.csproj" />
```
