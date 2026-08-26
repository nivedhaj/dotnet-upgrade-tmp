# ct.stu RavenDB → PostgreSQL Migration Report

- Generated (UTC): `2026-08-25T13:26:19.5532528Z`
- Input: `/home/nivedha/Downloads/ct-stu-dotnet10-postgres-migration-with-reports/ct-stu-dotnet10-postgres/Legacy/ct.stu`
- Files scanned: **110**
- Files with migration points: **13**
- Detected migration points: **526**

## `Legacy/ct.stu/_Core/Controllers/_MonitorController.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 7 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 8 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |

## `Legacy/ct.stu/_Core/Repo/Db/Command/PingCmd.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 3 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 26 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 26 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 27 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 27 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |

## `Legacy/ct.stu/_Core/Repo/Db/Index/Ping_IX.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 2 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |

## `Legacy/ct.stu/_Core/Repo/Db/Query/PingQry.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 4 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 24 | `_session.Query<Ping, Ping_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 24 | `Query` | `EF Core IQueryable<T>` | MEDIUM |

## `Legacy/ct.stu/_Core/Security/UserContext.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 10 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 11 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 362 | `_session.Load<dynamic>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 362 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 363 | `_session.Load<dynamic>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 363 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 379 | `_session.Advanced.Clear` | `Review for PostgreSQL equivalent` | MEDIUM |
| 506 | `store.OpenSession` | `Review for PostgreSQL equivalent` | MEDIUM |
| 547 | `_session.Advanced.ExternalState.Add` | `Review for PostgreSQL equivalent` | MEDIUM |
| 550 | `Query` | `EF Core IQueryable<T>` | MEDIUM |

## `Legacy/ct.stu/Repo/Db/Command/StudentCmd.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 8 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 9 | `using Raven.Client.Documents.Linq` | `Review for PostgreSQL equivalent` | HIGH |
| 10 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 88 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 88 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 122 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 122 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 123 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 123 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 151 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 151 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 156 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 156 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 157 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 157 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 172 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 172 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 178 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 178 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 179 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 179 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 201 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 201 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 207 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 207 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 208 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 208 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 235 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 235 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 250 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 250 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 251 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 251 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 272 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 272 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 274 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 274 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 275 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 275 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 287 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 287 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 289 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 289 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 290 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 290 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 303 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 303 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 305 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 305 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 306 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 306 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 319 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 319 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 321 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 321 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 322 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 322 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 335 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 335 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 339 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 339 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 340 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 340 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 366 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 366 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 372 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 372 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 373 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 373 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 404 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 404 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 411 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 411 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 421 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 421 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 422 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 422 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 437 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 437 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 444 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 444 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 454 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 454 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 455 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 455 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 468 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 468 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 471 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 471 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 472 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 472 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 485 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 485 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 488 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 488 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 489 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 489 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 502 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 502 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 505 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 505 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 506 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 506 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 519 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 519 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 522 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 522 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 523 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 523 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 539 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 539 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 546 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 546 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 555 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 555 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 556 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 556 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 581 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 581 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 584 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 584 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 585 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 585 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 598 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 598 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 601 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 601 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 602 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 602 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 624 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 624 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 625 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 625 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 652 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 652 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 653 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 653 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 719 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 719 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 730 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 730 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 731 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 731 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 761 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 761 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 769 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 769 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 770 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 770 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 785 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 785 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 796 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 796 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 797 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 797 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 818 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 818 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 826 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 826 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 827 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 827 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 844 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 844 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 846 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 846 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    .Where(o => o.StudentId.StartsWith(student_id_prefix))
                    .OrderByDescending(o => o.StudentId)
                    .Select(o => o.StudentId)
                    .FirstOrDefault` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    .Where(o => o.StudentId.StartsWith(student_id_prefix))
                    .OrderByDescending(o => o.StudentId)
                    .Select` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    .Where(o => o.StudentId.StartsWith(student_id_prefix))
                    .OrderByDescending` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where(o => o.InstId == inst_id)
                    .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                    .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 917 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 951 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 951 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.AdmissionNo.StartsWith(prefix))
                                     .OrderByDescending(o => o.AdmissionNo)
                                     .Select(o => o.AdmissionNo)
                                     .FirstOrDefault` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.AdmissionNo.StartsWith(prefix))
                                     .OrderByDescending(o => o.AdmissionNo)
                                     .Select` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where(o => o.AdmissionNo.StartsWith(prefix))
                                     .OrderByDescending` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where(o => o.InstId == inst_id)
                                     .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                     .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 974 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1066 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1066 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1077 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1100 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>` | `EF Core IQueryable<T>` | MEDIUM |
| 1100 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene` | `EF Core IQueryable<T>` | MEDIUM |
| 1100 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1105 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1186 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1186 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1238 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>` | `EF Core IQueryable<T>` | MEDIUM |
| 1238 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1238 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1238 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>` | `EF Core IQueryable<T>` | MEDIUM |
| 1238 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1244 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1284 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>` | `EF Core IQueryable<T>` | MEDIUM |
| 1284 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene` | `EF Core IQueryable<T>` | MEDIUM |
| 1284 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1289 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1322 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>` | `EF Core IQueryable<T>` | MEDIUM |
| 1322 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene` | `EF Core IQueryable<T>` | MEDIUM |
| 1322 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1327 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1368 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>` | `EF Core IQueryable<T>` | MEDIUM |
| 1368 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1368 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1368 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>` | `EF Core IQueryable<T>` | MEDIUM |
| 1368 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1374 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1412 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1412 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1420 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1420 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1421 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1421 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1433 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1433 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1442 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1442 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1443 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1443 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1455 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1455 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1464 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1464 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1465 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1465 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1477 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1477 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1486 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1486 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1487 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1487 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1499 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1499 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1508 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1508 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1509 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1509 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1521 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1521 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1530 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1530 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1531 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1531 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1543 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1543 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1552 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1552 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1553 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1553 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1565 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1565 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1574 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1574 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1575 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1575 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1587 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1587 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1596 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1596 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1597 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1597 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1609 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1609 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1618 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1618 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1619 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1619 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1631 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1631 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1640 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1640 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1641 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1641 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1653 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1653 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1662 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1662 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1663 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1663 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1675 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1675 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1684 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1684 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1685 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1685 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1699 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1699 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1719 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1719 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1720 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1720 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1727 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1727 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1746 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1746 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1747 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1747 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1754 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1754 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1779 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1779 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1781 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1781 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1786 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1786 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1798 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1798 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1800 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1800 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1805 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1805 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1820 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1820 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1821 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1821 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1829 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1829 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1840 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1840 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1841 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1841 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1860 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1860 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1861 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1861 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1869 | `_session.Query<Student_Essentials_IX.Result, Student_Essentials_IX>()
                            .Where(o => o.StudentId.Equals(studentId))
                            .OfType<StudentEssentials>()
                            .FirstOrDefault` | `EF Core IQueryable<T>` | MEDIUM |
| 1869 | `_session.Query<Student_Essentials_IX.Result, Student_Essentials_IX>()
                            .Where(o => o.StudentId.Equals(studentId))
                            .OfType<StudentEssentials>` | `EF Core IQueryable<T>` | MEDIUM |
| 1869 | `_session.Query<Student_Essentials_IX.Result, Student_Essentials_IX>()
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1869 | `_session.Query<Student_Essentials_IX.Result, Student_Essentials_IX>` | `EF Core IQueryable<T>` | MEDIUM |
| 1869 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1874 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1874 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1875 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1875 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1883 | `_session.Load<StudentEssentials>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1883 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1894 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1894 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1895 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1895 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1902 | `_session.Load<StudentEssentials>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1902 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1910 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1910 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1911 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1911 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1917 | `_session.Load<StudentEssentials>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1917 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1925 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1925 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1926 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1926 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1932 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1932 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1937 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1937 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1938 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1938 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1943 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1943 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1948 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1948 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1949 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1949 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1954 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1954 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1959 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1959 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1960 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1960 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1965 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1965 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1970 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1970 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1971 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1971 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1976 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1976 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1981 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1981 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1982 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1982 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1987 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1987 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1992 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1992 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 1993 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1993 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 1998 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1998 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 2003 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2003 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2004 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 2004 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 2026 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2026 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2027 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 2027 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 2031 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 2031 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 2036 | `_session.Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2036 | `Store` | `IStudentRepository.AddAsync(...)` | HIGH |
| 2037 | `_session.SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |
| 2037 | `SaveChanges` | `AppDbContext.SaveChangesAsync()` | HIGH |

## `Legacy/ct.stu/Repo/Db/Index/AttendanceEvent_IX.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 3 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |

## `Legacy/ct.stu/Repo/Db/Index/Course_IX.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 4 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |

## `Legacy/ct.stu/Repo/Db/Index/Student_IX.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 2 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |
| 108 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 109 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 110 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |

## `Legacy/ct.stu/Repo/Db/Index/StudentEssentials_IX.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 3 | `using Raven.Client.Documents.Indexes` | `Review for PostgreSQL equivalent` | HIGH |

## `Legacy/ct.stu/Repo/Db/Query/StudentQry.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 6 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 7 | `using Raven.Client.Documents.Linq` | `Review for PostgreSQL equivalent` | HIGH |
| 8 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 75 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 86 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 160 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 164 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 166 | `_session.Query<Course>().Where(o => o.InstId == _uc.InstId && o.Status == CourseStatusEnum.Active).ToList` | `EF Core IQueryable<T>` | MEDIUM |
| 166 | `_session.Query<Course>().Where` | `EF Core IQueryable<T>` | MEDIUM |
| 166 | `_session.Query<Course>` | `EF Core IQueryable<T>` | MEDIUM |
| 166 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 186 | `_session.Query<T, Student_Inst_Index>()
                        .Where(filterByInstId)
                        .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 186 | `_session.Query<T, Student_Inst_Index>()
                        .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 186 | `_session.Query<T, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 186 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 208 | `Statistics` | `Review for PostgreSQL equivalent` | MEDIUM |
| 224 | `_session.Query<T, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 224 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 225 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 227 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 228 | `Search` | `PostgreSQL ILIKE / full-text search` | MEDIUM |
| 254 | `Statistics` | `Review for PostgreSQL equivalent` | MEDIUM |
| 289 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 289 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 302 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 302 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 312 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 400 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where(o => o.Date == _dt)
                            .OfType<AttendanceEvent>` | `EF Core IQueryable<T>` | MEDIUM |
| 400 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where(o => o.InstId.Equals(instId))
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 400 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>()
                            .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 400 | `_session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>` | `EF Core IQueryable<T>` | MEDIUM |
| 400 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 406 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 447 | `_session.Query<Course>().Where(o => o.InstId == _uc.InstId).ToList` | `EF Core IQueryable<T>` | MEDIUM |
| 447 | `_session.Query<Course>().Where` | `EF Core IQueryable<T>` | MEDIUM |
| 447 | `_session.Query<Course>` | `EF Core IQueryable<T>` | MEDIUM |
| 447 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 507 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 507 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 539 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where(o => o.InstId.Equals(instId))
                                .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                .Where(o => o.Tags.Contains(tags))
                                .OfType<Student>` | `EF Core IQueryable<T>` | MEDIUM |
| 539 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where(o => o.InstId.Equals(instId))
                                .Where(o => o.EnrollmentStatus == EnrollmentStatusEnum.Active)
                                .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 539 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where(o => o.InstId.Equals(instId))
                                .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 539 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 539 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 539 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 546 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 576 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 584 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 643 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>()
                                .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 643 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 643 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 646 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 717 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 717 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 724 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 785 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 803 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 803 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 835 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 889 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 913 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 939 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1031 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene("Name", predicate)
                            .OfType<Student>` | `EF Core IQueryable<T>` | MEDIUM |
| 1031 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>()
                            .WhereLucene` | `EF Core IQueryable<T>` | MEDIUM |
| 1031 | `_session.Advanced
                            .DocumentQuery<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1036 | `_session.Advanced.Stream` | `Review for PostgreSQL equivalent` | MEDIUM |
| 1071 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1071 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1081 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1090 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1203 | `_session.Query<BaselineData>()
                              .Where(o => o.OwnerId == instId)
                              .ToList` | `EF Core IQueryable<T>` | MEDIUM |
| 1203 | `_session.Query<BaselineData>()
                              .Where` | `EF Core IQueryable<T>` | MEDIUM |
| 1203 | `_session.Query<BaselineData>` | `EF Core IQueryable<T>` | MEDIUM |
| 1203 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1220 | `_session.Load<BaselineData>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1220 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |

## `Legacy/ct.stu/Service/ReportService.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 13 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 14 | `using Raven.Client.Documents.Linq` | `Review for PostgreSQL equivalent` | HIGH |
| 15 | `using Raven.Client.Documents.Session` | `Review for PostgreSQL equivalent` | HIGH |
| 463 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 864 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1334 | `_session.Query<Student_Inst_Index.Result, Student_Inst_Index>` | `EF Core IQueryable<T>` | MEDIUM |
| 1334 | `Query` | `EF Core IQueryable<T>` | MEDIUM |
| 1430 | `_session.Load<Student>` | `IStudentRepository.GetByIdAsync(...)` | HIGH |
| 1430 | `Load` | `IStudentRepository.GetByIdAsync(...)` | HIGH |

## `Legacy/ct.stu/Startup.cs`

| Line | RavenDB API | PostgreSQL replacement | Confidence |
|---:|---|---|---|
| 16 | `using Raven.Client.Documents` | `Review for PostgreSQL equivalent` | HIGH |
| 84 | `createDocumentStore` | `IStudentRepository.AddAsync(...)` | HIGH |
| 85 | `ctx.GetService<IDocumentStore>().OpenSession` | `IStudentRepository.AddAsync(...)` | HIGH |
| 85 | `ctx.GetService<IDocumentStore>` | `IStudentRepository.AddAsync(...)` | HIGH |
| 148 | `new DocumentStore` | `IStudentRepository.AddAsync(...)` | HIGH |
| 157 | `new DocumentStore` | `IStudentRepository.AddAsync(...)` | HIGH |

## General migration guidance

- `IDocumentSession.Load<T>` → repository `GetByIdAsync`
- `Store` / `SaveChanges` → repository add/update + `SaveChangesAsync`
- `Query<T>` / LINQ → EF Core `IQueryable<T>`
- Raven `Search` → PostgreSQL `ILIKE` or full-text search
- Raven indexes → PostgreSQL indexes / views
- Raven `Include` / document graph → relational JOINs / EF Core `Include`
