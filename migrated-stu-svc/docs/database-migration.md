# Database Migration Notes

## Raven -> PostgreSQL mapping applied in this pass

| Raven concept | PostgreSQL replacement |
|---|---|
| Document store / session | `CtDbContext` (EF Core) |
| `Store` / `SaveChanges` | `DbSet.Add` / `SaveChangesAsync` |
| `LoadDocument` (cross-document joins) | SQL joins / EF `Include` / projection |
| Student search index | `student_search_view` + pg_trgm GIN indexes |
| Facet queries | `student_institute_summary_view` (GROUP BY aggregation) — only a first slice; full facet dimension set (course/section/tags) from spec section 44 is NOT yet built |
| Raven patch operations | EF entity mutation + `SaveChangesAsync`, or targeted SQL `UPDATE` for bulk ops (none needed yet at this scale) |

## How to apply the schema

This project intentionally ships hand-written SQL (`database/001_schema.sql`
through `004_triggers.sql`) rather than an EF Core migration bundle, so the
schema is reviewable independent of the ORM. To adopt EF migrations instead:

```
dotnet ef migrations add InitialCreate --project ct.csproj
dotnet ef database update --project ct.csproj
```

If you do this, treat the SQL files as the source of truth to diff the
generated migration against, then delete the SQL files to avoid two
competing schema sources.

To apply the hand-written SQL directly:

```
psql "$CONNECTION_STRING" -f database/001_schema.sql
psql "$CONNECTION_STRING" -f database/002_indexes.sql
psql "$CONNECTION_STRING" -f database/003_views.sql
psql "$CONNECTION_STRING" -f database/004_triggers.sql
```

**Neither path has been executed in this environment** — there is no
PostgreSQL instance available in the sandbox that produced this code. Run
and verify before trusting it in any environment that matters.

## Tables NOT yet created
Attendance, fees, opening balances, IEP, student essentials, documents, and
baseline data tables (spec sections 63–68) are specified but not yet
implemented. Do not point the Attendance/Fees/Examinations modules at this
schema until those tables exist — there is currently nothing for them to
reference.
