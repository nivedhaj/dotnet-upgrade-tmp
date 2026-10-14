# Agentic Migration Specs (self-contained)

Each `*-agentic-migration-spec.md` includes:

1. Platform target (.NET 10 / C# 14 / EF Postgres)
2. Full endpoint inventory
3. Cmd/Qry signatures
4. Domain properties & enums
5. **Business rules (authoritative)** — cross-cutting + per-method signals from legacy Cmd/Handlers

**No legacy zip is required** if the agent follows these files + `00-AGENT-PROTOCOL.md`.

| File | Module |
|---|---|
| [fee-agentic-migration-spec.md](./fee-agentic-migration-spec.md) | Fee |
| [stu-agentic-migration-spec.md](./stu-agentic-migration-spec.md) | Student |
| [staff-agentic-migration-spec.md](./staff-agentic-migration-spec.md) | Staff |
| [scheduler-agentic-migration-spec.md](./scheduler-agentic-migration-spec.md) | Scheduler |
| [asset-agentic-migration-spec.md](./asset-agentic-migration-spec.md) | Asset |
| [accounting-agentic-migration-spec.md](./accounting-agentic-migration-spec.md) | Accounting |
| [admission-agentic-migration-spec.md](./admission-agentic-migration-spec.md) | Admission |
| [library-agentic-migration-spec.md](./library-agentic-migration-spec.md) | Library |
| [connect-agentic-migration-spec.md](./connect-agentic-migration-spec.md) | Connect |
| [gr-agentic-migration-spec.md](./gr-agentic-migration-spec.md) | Gr |
| [arte-agentic-migration-spec.md](./arte-agentic-migration-spec.md) | Arte |
| [acad-agentic-migration-spec.md](./acad-agentic-migration-spec.md) | Acad |