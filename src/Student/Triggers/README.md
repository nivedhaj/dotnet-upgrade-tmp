# Student triggers / views (Postgres + Marten)

## Pattern

| Piece | Location | Role |
|-------|----------|------|
| **View** | `Student/Views/StudentFeeSummary.cs` | Document type stored in Student DB; read-only for Student code |
| **Trigger / publisher** | `Fees/Triggers/FeeSummaryEtl.cs` | After Fees writes, upserts `StudentFeeSummary` into Student store |

## Raven → Postgres change

- **Before:** RavenDB ETL task (Fees DB → Student DB) configured in `FeeSummaryEtl.EnsureConfigured`.
- **After:** `FeeSummaryEtl.PublishAsync` (application publish) or optional SQL TRIGGER. Marten stores both document types in PostgreSQL (`jsonb`).

## How Student reads the view

```csharp
var summary = session.Load<StudentFeeSummary>(studentId);
// or
var list = session.Query<StudentFeeSummary>().Where(x => x.InstId == instId).ToList();
```

## Wiring

1. `StudentModule` registers `StudentFeeSummary` with identity `StudentId`.
2. `FeesCmd` (after save) resolves Student `IDocumentStore` and calls `FeeSummaryEtl.PublishAsync`.
3. No Raven Studio ETL task is required.
