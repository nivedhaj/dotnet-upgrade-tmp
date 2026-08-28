# Migrated STU service architecture

The source tree follows a modular-monolith layout based on business capabilities rather than the old `Services/Ct.Stu/Ct.Stu.*` hierarchy.

```text
src/
├── Host/
│   ├── Program.cs
│   ├── MonitorController.cs
│   └── appsettings.json
│
├── Modules/
│   ├── Academics/
│   │   ├── Api/
│   │   ├── Application/
│   │   │   └── Students/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── AcademicsModule.cs
│   │
│   ├── Admissions/
│   ├── Attendance/
│   ├── Fees/
│   │   ├── Api/
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── FeesModule.cs
│   └── Examinations/
│
├── Infrastructure/
│   ├── DependencyInjection.cs
│   └── Persistence/
│       ├── AppDbContext.cs
│       ├── Configurations/
│       │   ├── Academics/
│       │   └── Fees/
│       └── Migrations/
│
└── BuildingBlocks/
    ├── Application/
    └── Infrastructure/
```

## Design decisions

- `Host` owns application startup and host-level endpoints/configuration.
- `Modules` owns business capabilities. `Academics` and `Fees` contain the currently migrated functionality.
- `Infrastructure/Persistence` is shared because the application uses one PostgreSQL database and one EF Core `AppDbContext`.
- Persistence configurations are grouped by module so adding Attendance, Admissions, or Examinations does not recreate a deep service hierarchy.
- EF Core migrations are kept in one shared `Infrastructure/Persistence/Migrations` location.
- `BuildingBlocks` contains cross-cutting primitives shared by modules.
- The application intentionally retains a single `src/ct.csproj`.
