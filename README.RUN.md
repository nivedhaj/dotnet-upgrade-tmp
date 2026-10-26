# Run & test updates (all modules)

Drop these files into the root of `ct-modular/` (overwrite `scripts/`, `Ct.Modular.sln`, `Makefile`, `docker-compose.yml`).

## What changed vs the earlier pack

| Item | Before | After |
|------|--------|--------|
| **Modules in .sln** | 6 (Connect, Fee, Gr, Stu, Staff, Scheduler) | **12** (+ Acad, Accounting, Admission, Arte, Asset, Library) |
| **Test projects** | 6 | **8** (+ Acad, Arte) |
| **run-tests.sh** | Hard-coded list | **Auto-discovers** every `tests/**/*.csproj` |
| **run-host.sh** | Generic | Lists modules from `Ct.Host.csproj` before build |

Host already referenced all 12 modules; only the **solution + scripts** were incomplete.

## Modules (source)

Acad · Accounting · Admission · Arte · Asset · Connect · Fee · Gr · Library · Scheduler · Staff · Stu

## Test suites present

| Folder | Project file |
|--------|----------------|
| `ct.connect.tests` | `ct.connect.tests.csproj` |
| `ct.fee.tests` | `ct.fee.tests.csproj` |
| `ct.gr.tests` | `ct.gr.tests.csproj` |
| `ct.stu.tests` | `ct.stu.tests.csproj` |
| `ct.staff.tests` | `ct.staff.tests.csproj` |
| `ct.scheduler.tests` | `ct.scheduler.tests.csproj` |
| `ct.acad.tests` | `Ct.Acad.Tests.csproj` |
| `ct.arte.tests` | `Ct.Arte.Tests.csproj` |

No dedicated test projects yet for **Library, Asset, Accounting, Admission** (source only).

## Commands

```bash
./scripts/setup-db.sh
./scripts/build-all.sh
./scripts/run-host.sh                 # http://localhost:5001/swagger

./scripts/run-tests.sh                # all discovered suites
./scripts/run-tests.sh fee
./scripts/run-tests.sh acad
./scripts/run-tests.sh help           # list modules
```

Or: `make db` · `make build` · `make host` · `make test` · `make test-fee`

## Are the old files “good enough”?

- **Yes for the six suites we generated** (connect/fee/gr/stu/staff/scheduler).
- **No for the full tree** in `ct-modular-combined-new.zip`: Acad/Arte tests and extra modules were missing from `run-tests.sh` and `.sln`, so `dotnet test` on the solution / `./scripts/run-tests.sh all` would skip them.

Use this overlay on the **new** combined package.
