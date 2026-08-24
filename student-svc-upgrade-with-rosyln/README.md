# Student Service – RavenDB → PostgreSQL POC

This POC is based on the uploaded `ct.stu` source. The old application is .NET Core 3.1 + RavenDB. The POC demonstrates a migration target of **.NET 10 / C# 14 + PostgreSQL**, while using **Roslyn as a migration bridge**.

## What is demonstrated

- `StudentService.Api`
  - ASP.NET Core / .NET 10 Web API
  - EF Core 10 + Npgsql
  - PostgreSQL relational model
  - `students` + `student_enrollments`
  - JSONB for intentionally flexible/legacy fields
  - Create, get, search, enable/disable, promote
- `StudentMigration.Roslyn`
  - Roslyn-based source analyzer
  - Scans old RavenDB code for:
    - `IDocumentSession`
    - `Load<T>()`
    - `Store()`
    - `SaveChanges()`
    - `Query<T>()`
    - `Statistics()`
    - Raven `Search()`
  - Produces a migration report and maps Raven operations to the new repository/service pattern.
- `LegacySample/StudentCmd.cs`
  - Small representative extract based on the uploaded Raven implementation.

## Important architecture point

Roslyn is **not used as a database adapter**. It is the bridge for the **code migration**:

```text
.NET Core 3.1 + RavenDB
        |
        | Roslyn analysis / classification
        v
Migration map + compatibility seams
        |
        v
.NET 10 + C# 14 + PostgreSQL
```

For a gradual migration, the existing controller can first call an interface such as `IStudentRepository`. The old Raven implementation and the new PostgreSQL implementation can both implement that interface. Roslyn helps identify and classify the old code so the replacement can be done method-by-method.

## PostgreSQL schema

The POC uses normalized tables:

```text
students
  1
  |
  +----< student_enrollments
```

Flexible fields such as domicile/documents/IEP can initially be kept as JSONB. They can later be normalized when their query/reporting requirements become clear.

## Run

1. Start PostgreSQL:

```bash
docker compose -f docker-compose.yml up -d
```

2. Run the API:

```bash
dotnet restore
dotnet run --project src/StudentService.Api
```

3. Run the Roslyn bridge against the legacy source:

```bash
dotnet run --project src/StudentMigration.Roslyn -- LegacySample/StudentCmd.cs
```

4. API examples:

```bash
curl http://localhost:5080/api/students
curl "http://localhost:5080/api/students?search=Ravi&instituteId=INST-001"

curl -X POST http://localhost:5080/api/students \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ravi Kumar",
    "dob": "2012-05-10",
    "email": "ravi@example.com",
    "mobile": "9876543210",
    "instituteId": "INST-001",
    "courseId": "COURSE-1",
    "courseName": "VI",
    "termName": "2026-27",
    "sectionName": "A",
    "branch": "",
    "studentId": "STU-001",
    "admissionNumber": "ADM-001",
    "applicationNumber": "APP-001",
    "doa": "2026-06-01"
  }'
```

## Migration sequence recommended for the real application

1. Freeze the Raven document contract.
2. Use Roslyn to inventory Raven calls and classify them as read/write/index/search.
3. Introduce repository interfaces in the existing application.
4. Keep Raven implementation behind the interface.
5. Add PostgreSQL implementation.
6. Run Raven and PostgreSQL in parallel for selected operations.
7. Backfill existing students into PostgreSQL.
8. Compare Raven/PostgreSQL results for important queries.
9. Switch reads first.
10. Switch writes.
11. Retire Raven indexes and then Raven persistence.

The POC intentionally does **not** attempt to automatically translate every Raven LINQ expression to SQL. Raven indexes and `Search()` calls often encode query behavior that must be designed explicitly in PostgreSQL.
