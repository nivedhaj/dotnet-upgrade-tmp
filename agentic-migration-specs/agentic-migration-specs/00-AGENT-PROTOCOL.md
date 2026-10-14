# CT Modular Migration — Agentic Protocol

## Goal
Migrate each legacy RavenDB (or event-sourced) service into a **.NET 10 / C# 14** modular project under `CtModularApp`, using **EF Core + PostgreSQL** only. No RavenDB. No Marten unless the product owner explicitly reintroduces event sourcing.

## Target stack
| Item | Value |
|---|---|
| Runtime | .NET 10 |
| Language | C# 14 (`nullable` enable, primary constructors OK) |
| Host | `Ct.Host` ASP.NET Core |
| Shared | `Ct.Shared` (headers, extensions) |
| Persistence | `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x |
| Events (optional) | RabbitMQ JSON publish — not Raven subscriptions |
| Auth context | Headers `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` |

## Module layout (mandatory)
```
Ct.{Module}/
  Ct.{Module}.csproj
  {Module}Module.cs          // Add{Module}Module + Map{Module}Endpoints
  Domain/                    // POCOs, enums
  Infrastructure/            // *DbContext, jsonb converters
  Application/               // *Service, *Cmd, *Qry facades
  Controllers/               // mirror legacy controller names where practical
```

## Extraction order (from old code)
1. **Controllers** → list every `[HttpGet|Post|Put|Delete]` route + action name. **Do not skip.**
2. **Repo/Db/Command (*Cmd.cs)** → write methods → `*Service` / `*Cmd` facade.
3. **Repo/Db/Query (*Qry.cs)** → read methods → `*Service` / `*Qry` facade.
4. **Repo/Resource** (or CQRS Aggregate) → domain entities + enums.
5. **Indexes** (`*_IX.cs`) → EF indexes or report projections (replace Raven map/reduce).
6. **Service/** (ReportService, RabbitMQ enqueue) → ReportService JSON/HTML stubs; `I*EventPublisher` for jobs.
7. **Startup/Program** → register `Add{Module}Module` + `AddApplicationPart` in `Ct.Host`.

## RavenDB → PostgreSQL mapping rules
| Raven pattern | Postgres / EF pattern |
|---|---|
| `IDocumentSession` Load/Query/Store | `DbContext` + `DbSet<T>` |
| Document id string | `Guid Id` (or keep string only if external refs require it) |
| Nested lists/dicts on document | `jsonb` columns + `HasConversion` + `ValueComparer` |
| Map/reduce index | LINQ group-by report methods or SQL views later |
| `session.Advanced.DocumentStore` | Connection string `Postgres` / `DefaultConnection` |
| Multi-tenant via `OwnerId` / `InstId` | Always filter `OwnerId == user.OwnerId` |
| Event sourcing stream (Asset/Library/Accounting legacy) | **Current-state tables** unless owner asks for event store |
| Rabbit `ct_xt` / `ct_x` jobs | Optional `I*EventPublisher` with same routing keys |

## Controller parity checklist
- [ ] Every legacy action method name exists on modular controllers (case-insensitive match OK for C#).
- [ ] Routes prefer legacy path segments (`disable`, `setTags`, …).
- Dual routes allowed: `api/{module}/...` and legacy `api/{Controller}`.
- Commented-out legacy endpoints may be restored as optional extras; document them.

## Anti-goals
- Do not leave Marten/Raven packages in csproj.
- Do not claim 1:1 print-engine HTML (Github StringTemplate) unless templates are provided.
- Do not skip Cmd/Qry methods that have no HTTP surface — expose as service methods and/or extra routes.

## Verification
```text
Compare: count(legacy Http* actions) vs count(modular Http* actions)
Target: 100% method-name coverage for active (non-commented) endpoints
```

## Deliverable per module
1. Spec file in `migration-specs/` (this folder).
2. `Ct.{Module}` project wired into Host.
3. Updated `CtModularApp-updated.zip`.


## Business rules section (mandatory in every module spec)

Each module spec contains a **"Business rules (authoritative)"** section derived from legacy Cmd/Qry/Handler bodies.

**Agent instructions:**
1. Endpoint tables define **names and routes only**.
2. **Business rules** define algorithms, preconditions, status changes, and side effects.
3. If a method has a rules entry, implement **exactly those constraints** (adapt Raven `session` calls to EF `SaveChangesAsync`).
4. If a method has **no** rules entry: OwnerId-scoped CRUD only; list it under `RULES NEEDED` in your summary.
5. Never invent fee math, seat counts, stock limits, or voucher balance rules beyond what is written.
6. Replace `session.Store` / `eventStore.Save` with EF; replace `Enque*` with optional `IEventPublisher` when a side effect is listed.
