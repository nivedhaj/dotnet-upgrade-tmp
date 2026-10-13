using System.Globalization;
using System.Text;
using Ct.Fee.Application.Integrations;
using Ct.Fee.Application.Policy;
using Ct.Fee.Domain;
using Ct.Fee.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Fee.Application;

/// <summary>
/// PostgreSQL-backed report engine replacing original RavenDB ReportService indexes.
/// Returns structured DTOs (JSON-friendly). HTML template rendering is left to the host.
/// </summary>
public sealed class ReportService(FeeDbContext db, ICurrentUserContext user, IStudentDirectory students, IFeePolicyEngine policy)
{
    // ===================== Fee structure =====================

    public async Task<IReadOnlyList<FeeStructureRowDto>> GetFeeStructureAsync(CancellationToken ct)
    {
        var fees = await ActiveFeesQuery()
            .Include(x => x.Installments).Include(x => x.Fines)
            .Include(x => x.Students).Include(x => x.Courses)
            .OrderBy(x => x.NameLower)
            .ToListAsync(ct);

        return fees.Select(f => new FeeStructureRowDto(
            f.Id, f.Name, f.DisplayText, f.Amount, f.CollectStudentWise, f.SpansAcademicYears,
            f.Tags, f.Courses.Select(c => c.CourseId).Order().ToList(),
            f.Students.Select(s => s.StudentId).Order().ToList(),
            f.Installments.OrderBy(i => i.InstallmentNo)
                .Select(i => new FeeInstallmentDto(i.InstallmentNo, i.Description, i.Amount, i.DueDate)).ToList(),
            f.Fines.OrderBy(i => i.FineNo)
                .Select(i => new FeeFineDto(i.FineNo, i.Description, i.Amount, i.OverdueByDays)).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyList<CoursewiseFeeStructureRowDto>> GetCoursewiseFeeStructureAsync(
        string? courseId, CancellationToken ct)
    {
        var q = ActiveFeesQuery().Include(x => x.Installments).Include(x => x.Courses).AsQueryable();
        if (!string.IsNullOrWhiteSpace(courseId))
            q = q.Where(x => x.Courses.Any(c => c.CourseId == courseId));

        var fees = await q.ToListAsync(ct);
        return fees.SelectMany(f =>
        {
            var courses = f.Courses.Select(c => c.CourseId).ToList();
            if (courses.Count == 0) courses.Add("");
            return courses
                .Where(c => string.IsNullOrWhiteSpace(courseId) || c == courseId)
                .SelectMany(c => f.Installments.OrderBy(i => i.InstallmentNo)
                    .Select(i => new CoursewiseFeeStructureRowDto(
                        c, f.Id, f.Name, i.InstallmentNo, i.Description, i.Amount, i.DueDate)));
        }).OrderBy(x => x.CourseId).ThenBy(x => x.FeeName).ThenBy(x => x.InstallmentNo).ToList();
    }

    // ===================== Expected fees =====================

    public async Task<IReadOnlyList<ExpectedFeeLineDto>> GetFeesExpectedAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var fees = await ActiveFeesQuery()
            .Include(x => x.Students).Include(x => x.Courses)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(filter?.FeeId) && Guid.TryParse(filter.FeeId, out var feeGuid))
            fees = fees.Where(f => f.Id == feeGuid).ToList();
        if (!string.IsNullOrWhiteSpace(filter?.CourseId))
            fees = fees.Where(f => f.Courses.Any(c => c.CourseId == filter.CourseId)).ToList();

        var paidByFee = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.InstallmentsPaid)
            .GroupBy(x => x.FeeDefinitionId)
            .Select(g => new { FeeId = g.Key, Paid = g.Sum(x => x.Amount) })
            .ToListAsync(ct);
        var paidMap = paidByFee.Where(x => x.FeeId.HasValue)
            .ToDictionary(x => x.FeeId!.Value, x => x.Paid);

        var result = new List<ExpectedFeeLineDto>();
        foreach (var f in fees)
        {
            var studentCount = f.CollectStudentWise
                ? f.Students.Count
                : Math.Max(f.Students.Count, 1); // course-wide: at least 1 if no roster stored
            if (!string.IsNullOrWhiteSpace(filter?.CourseId))
            {
                // restrict student count if course filter present and students tagged
                studentCount = f.CollectStudentWise
                    ? f.Students.Count
                    : studentCount;
            }
            var expected = f.Amount * studentCount;
            var collected = paidMap.GetValueOrDefault(f.Id, 0m);
            var course = f.Courses.Select(c => c.CourseId).FirstOrDefault() ?? filter?.CourseId;
            result.Add(new ExpectedFeeLineDto(
                f.Id, f.Name, f.DisplayText, f.Amount, studentCount,
                Round(expected), Round(collected), Round(expected - collected), course));
        }
        return result.OrderBy(x => x.FeeName).ToList();
    }

    // ===================== Datewise collection =====================

    public async Task<DatewiseCollectionReportDto> GetDatewiseFeeCollectionAsync(
        DateOnly from, DateOnly to, CancellationToken ct)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var txs = await db.FeeTransactions.AsNoTracking()
            .Include(x => x.InstallmentsPaid)
            .Where(x => x.Status == TransactionStatus.Completed
                        && !x.IsOpeningBalanceAdjusted
                        && x.TxDate >= fromDt && x.TxDate <= toDt)
            .ToListAsync(ct);

        var feeIds = txs.SelectMany(t => t.InstallmentsPaid)
            .Where(i => i.FeeDefinitionId.HasValue)
            .Select(i => i.FeeDefinitionId!.Value)
            .Distinct().ToList();
        var feeNames = await db.Fees.AsNoTracking()
            .Where(f => feeIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => f.Name, ct);

        var rows = new List<DatewiseCollectionRowDto>();
        foreach (var tx in txs)
        {
            var date = DateOnly.FromDateTime(tx.TxDate.UtcDateTime);
            if (tx.InstallmentsPaid.Count == 0)
            {
                rows.Add(new DatewiseCollectionRowDto(date, null, "(unallocated)", tx.Amount, 1, tx.PaymentMode));
                continue;
            }
            foreach (var grp in tx.InstallmentsPaid.GroupBy(i => i.FeeDefinitionId))
            {
                var name = grp.Key.HasValue && feeNames.TryGetValue(grp.Key.Value, out var n) ? n : "(unknown)";
                rows.Add(new DatewiseCollectionRowDto(
                    date, grp.Key, name, Round(grp.Sum(x => x.Amount)), 1, tx.PaymentMode));
            }
        }

        // collapse same date+fee
        var collapsed = rows
            .GroupBy(r => new { r.Date, r.FeeId, r.FeeName, r.PaymentMode })
            .Select(g => new DatewiseCollectionRowDto(
                g.Key.Date, g.Key.FeeId, g.Key.FeeName,
                Round(g.Sum(x => x.Amount)), g.Sum(x => x.TransactionCount), g.Key.PaymentMode))
            .OrderBy(x => x.Date).ThenBy(x => x.FeeName)
            .ToList();

        var cols = collapsed.Select(x => x.FeeName).Distinct().OrderBy(x => x).ToList();
        return new DatewiseCollectionReportDto(from, to, cols, collapsed, Round(collapsed.Sum(x => x.Amount)));
    }

    public async Task<GroupedDatewiseCollectionDto> GetDatewiseFeeCollectionGroupedAsync(
        DateOnly from, DateOnly to, CancellationToken ct)
    {
        var report = await GetDatewiseFeeCollectionAsync(from, to, ct);
        var feeNames = report.FeeColumns;
        var byDate = report.Rows.GroupBy(r => r.Date).OrderBy(g => g.Key);
        var rows = byDate.Select(g =>
        {
            var map = feeNames.ToDictionary(f => f, f => 0m);
            foreach (var r in g)
                map[r.FeeName] = map.GetValueOrDefault(r.FeeName) + r.Amount;
            return new GroupedDateRowDto(g.Key, map, Round(g.Sum(x => x.Amount)));
        }).ToList();

        return new GroupedDatewiseCollectionDto(
            from, to,
            rows.Select(r => r.Date).ToList(),
            feeNames,
            rows,
            Round(rows.Sum(r => r.DayTotal)));
    }

    // ===================== Defaulters =====================

    public async Task<DefaultersReportDto> GetFeesDefaultersAsync(
        string? courseId, DateOnly? asOf, int version, bool showOpeningBalance, CancellationToken ct)
    {
        var asOfDate = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var fees = await ActiveFeesQuery()
            .Include(x => x.Installments).Include(x => x.Students).Include(x => x.Courses)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(courseId))
            fees = fees.Where(f => f.Courses.Any(c => c.CourseId == courseId) || f.Courses.Count == 0).ToList();

        // All completed installment payments
        var paidQuery = db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed);
        if (!showOpeningBalance)
            paidQuery = paidQuery.Where(x => !x.IsOpeningBalanceAdjusted);

        var paidLines = await paidQuery
            .SelectMany(x => x.InstallmentsPaid.Select(i => new
            {
                x.StudentId,
                i.FeeDefinitionId,
                i.InstallmentNo,
                i.Amount,
                x.TxDate
            }))
            .ToListAsync(ct);

        // Students in scope = those on fee student lists, or who have paid something
        var studentIds = fees.SelectMany(f => f.Students.Select(s => s.StudentId))
            .Union(paidLines.Select(p => p.StudentId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var students = new List<DefaulterStudentDto>();
        foreach (var studentId in studentIds)
        {
            var installments = new List<DefaulterInstallmentDto>();
            foreach (var fee in fees)
            {
                // applicable if course-wide or student listed
                var applicable = !fee.CollectStudentWise
                                 || fee.Students.Any(s => string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase));
                if (!applicable) continue;

                foreach (var inst in fee.Installments.OrderBy(i => i.InstallmentNo))
                {
                    // v2: only installments due on or before asOf
                    if (version >= 2 && inst.DueDate.HasValue && inst.DueDate.Value > asOfDate)
                        continue;

                    var paid = paidLines
                        .Where(p => string.Equals(p.StudentId, studentId, StringComparison.OrdinalIgnoreCase)
                                    && p.FeeDefinitionId == fee.Id
                                    && p.InstallmentNo == inst.InstallmentNo)
                        .Sum(p => p.Amount);

                    var balance = Round(inst.Amount - paid);
                    if (balance <= 0) continue;

                    installments.Add(new DefaulterInstallmentDto(
                        fee.Id, fee.Name, inst.InstallmentNo, inst.Description,
                        inst.DueDate, inst.Amount, Round(paid), balance));
                }
            }

            if (installments.Count == 0) continue;
            var totalDue = Round(installments.Sum(i => i.Amount));
            var totalPaid = Round(installments.Sum(i => i.Paid));
            var course = fees.SelectMany(f => f.Courses).Select(c => c.CourseId).FirstOrDefault() ?? courseId;
            students.Add(new DefaulterStudentDto(
                studentId, course, totalDue, totalPaid, Round(totalDue - totalPaid), installments));
        }

        students = students.OrderByDescending(s => s.Balance).ThenBy(s => s.StudentId).ToList();
        return new DefaultersReportDto(
            courseId, asOfDate, version, showOpeningBalance, students,
            Round(students.Sum(s => s.Balance)));
    }

    // ===================== Transactions / receipts =====================

    public async Task<IReadOnlyList<ReceiptTransactionRowDto>> GetReceiptTransactionsAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.InstallmentsPaid)
            .Include(x => x.FinesPaid)
            .Include(x => x.Discounts)
            .Include(x => x.Adjustments)
            .AsQueryable();

        if (filter?.FromDate is not null)
            q = q.Where(x => x.TxDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TxDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(filter?.StudentId))
            q = q.Where(x => x.StudentId == filter.StudentId);
        if (!string.IsNullOrWhiteSpace(filter?.PaymentMode))
            q = q.Where(x => x.PaymentMode == filter.PaymentMode);
        if (filter?.IncludeInactive != true)
            q = q.Where(x => x.Status == TransactionStatus.Completed);

        var list = await q.OrderByDescending(x => x.TxDate).ToListAsync(ct);
        return list.Select(MapReceipt).ToList();
    }

    public async Task<CsvReportDto> GetReceiptTransactionsCsvAsync(ReportFilterRequest? filter, CancellationToken ct)
    {
        var rows = await GetReceiptTransactionsAsync(filter, ct);
        var sb = new StringBuilder();
        sb.AppendLine("TxNo,TxDate,StudentId,PaymentMode,RefNo,ReceivedInAccount,Amount,Status,IsOpeningBalance");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                Csv(r.TxNo),
                r.TxDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Csv(r.StudentId),
                Csv(r.PaymentMode),
                Csv(r.RefNo),
                Csv(r.ReceivedInAccount),
                r.Amount.ToString(CultureInfo.InvariantCulture),
                Csv(r.Status),
                r.IsOpeningBalanceAdjusted));
        }
        return new CsvReportDto("ReceiptTransactions.csv", sb.ToString(), rows);
    }

    public async Task<StudentwiseTransactionReportDto> GetStudentwiseTransactionsAsync(
        string studentId, CancellationToken ct)
    {
        var rows = await GetReceiptTransactionsAsync(
            new ReportFilterRequest(null, null, null, studentId, null, null, null, true), ct);
        return new StudentwiseTransactionReportDto(
            studentId, rows,
            Round(rows.Sum(r => r.Amount)),
            Round(rows.SelectMany(r => r.Discounts).Sum(d => d.Amount)),
            Round(rows.SelectMany(r => r.Adjustments).Sum(a => a.Amount)));
    }

    public async Task<IReadOnlyList<StudentwiseTransactionReportDto>> GetCourseStudentwiseTransactionsAsync(
        string? courseId, CancellationToken ct)
    {
        // Students linked to fees for the course
        var studentIds = await db.Fees.AsNoTracking()
            .Where(f => string.IsNullOrWhiteSpace(courseId) || f.Courses.Any(c => c.CourseId == courseId))
            .SelectMany(f => f.Students.Select(s => s.StudentId))
            .Distinct()
            .ToListAsync(ct);

        // Also include students who paid against those fees
        var feeIds = await db.Fees.AsNoTracking()
            .Where(f => string.IsNullOrWhiteSpace(courseId) || f.Courses.Any(c => c.CourseId == courseId))
            .Select(f => f.Id)
            .ToListAsync(ct);

        var paidStudentIds = await db.FeeTransactions.AsNoTracking()
            .Where(t => t.InstallmentsPaid.Any(i => i.FeeDefinitionId != null && feeIds.Contains(i.FeeDefinitionId.Value)))
            .Select(t => t.StudentId)
            .Distinct()
            .ToListAsync(ct);

        var all = studentIds.Union(paidStudentIds, StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
        var result = new List<StudentwiseTransactionReportDto>();
        foreach (var sid in all)
            result.Add(await GetStudentwiseTransactionsAsync(sid, ct));
        return result;
    }

    public async Task<IReadOnlyList<AdjustmentTransactionRowDto>> GetAdjustmentTransactionsAsync(
        string? courseId, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.Adjustments)
            .Where(x => x.Adjustments.Any());

        if (!string.IsNullOrWhiteSpace(courseId))
        {
            var feeIds = await db.Fees.AsNoTracking()
                .Where(f => f.Courses.Any(c => c.CourseId == courseId))
                .Select(f => f.Id).ToListAsync(ct);
            q = q.Where(x => x.InstallmentsPaid.Any(i => i.FeeDefinitionId != null && feeIds.Contains(i.FeeDefinitionId.Value)));
        }

        var txs = await q.OrderByDescending(x => x.TxDate).ToListAsync(ct);
        return txs.SelectMany(t => t.Adjustments.Select(a =>
            new AdjustmentTransactionRowDto(t.Id, t.TxNo, t.TxDate, t.StudentId, a.Reason, a.Amount))).ToList();
    }

    // ===================== Fees paid =====================

    public async Task<FeesPaidReportDto> GetFeesPaidAsync(
        DateOnly from, DateOnly to, string? courseId, IReadOnlyList<FeeInstRef>? feeInsts, CancellationToken ct)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.InstallmentsPaid)
            .Where(x => x.Status == TransactionStatus.Completed
                        && x.TxDate >= fromDt && x.TxDate <= toDt);

        var txs = await q.ToListAsync(ct);
        var feeIds = txs.SelectMany(t => t.InstallmentsPaid)
            .Where(i => i.FeeDefinitionId.HasValue).Select(i => i.FeeDefinitionId!.Value).Distinct().ToList();
        var fees = await db.Fees.AsNoTracking()
            .Include(f => f.Courses).Include(f => f.Installments)
            .Where(f => feeIds.Contains(f.Id))
            .ToListAsync(ct);
        var feeMap = fees.ToDictionary(f => f.Id);

        if (!string.IsNullOrWhiteSpace(courseId))
            fees = fees.Where(f => f.Courses.Any(c => c.CourseId == courseId)).ToList();

        var feeInstFilter = feeInsts?.ToHashSet() ?? null;
        var rows = new List<FeesPaidRowDto>();
        foreach (var tx in txs)
        {
            foreach (var line in tx.InstallmentsPaid)
            {
                if (!line.FeeDefinitionId.HasValue || !feeMap.TryGetValue(line.FeeDefinitionId.Value, out var fee))
                    continue;
                if (!string.IsNullOrWhiteSpace(courseId) && fee.Courses.All(c => c.CourseId != courseId))
                    continue;
                if (feeInstFilter is not null &&
                    !feeInstFilter.Contains(new FeeInstRef(line.FeeDefinitionId.Value, line.InstallmentNo)))
                    continue;

                var inst = fee.Installments.FirstOrDefault(i => i.InstallmentNo == line.InstallmentNo);
                rows.Add(new FeesPaidRowDto(
                    tx.StudentId,
                    fee.Courses.Select(c => c.CourseId).FirstOrDefault(),
                    fee.Id, fee.Name, line.InstallmentNo,
                    inst?.Amount ?? line.Amount,
                    line.Amount, tx.TxDate, tx.PaymentMode, tx.TxNo));
            }
        }

        rows = rows.OrderBy(r => r.StudentId).ThenBy(r => r.FeeName).ThenBy(r => r.InstallmentNo).ToList();
        return new FeesPaidReportDto(from, to, courseId, rows, Round(rows.Sum(r => r.AmountPaid)));
    }

    public async Task<CsvReportDto> GetFeesPaidCsvAsync(
        DateOnly from, DateOnly to, string? courseId, IReadOnlyList<FeeInstRef>? feeInsts, CancellationToken ct)
    {
        var report = await GetFeesPaidAsync(from, to, courseId, feeInsts, ct);
        var sb = new StringBuilder();
        sb.AppendLine("StudentId,CourseId,FeeName,InstallmentNo,AmountDue,AmountPaid,PaidOn,PaymentMode,TxNo");
        foreach (var r in report.Rows)
        {
            sb.AppendLine(string.Join(',',
                Csv(r.StudentId), Csv(r.CourseId), Csv(r.FeeName), r.InstallmentNo,
                r.AmountDue.ToString(CultureInfo.InvariantCulture),
                r.AmountPaid.ToString(CultureInfo.InvariantCulture),
                r.PaidOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "",
                Csv(r.PaymentMode), Csv(r.TxNo)));
        }
        return new CsvReportDto("FeesPaid.csv", sb.ToString(), report);
    }

    // ===================== Breakups / fine / late / timeline =====================

    public async Task<IReadOnlyList<BreakupRowDto>> GetBreakUpsAsync(ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var datewise = await GetDatewiseFeeCollectionAsync(from, to, ct);
        return datewise.Rows
            .GroupBy(r => r.FeeName)
            .Select(g => new BreakupRowDto("Fee", g.Key, Round(g.Sum(x => x.Amount)), g.Sum(x => x.TransactionCount)))
            .OrderByDescending(x => x.Amount)
            .ToList();
    }

    public async Task<IReadOnlyList<FineCollectedRowDto>> GetFineCollectedAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.FinesPaid)
            .Where(x => x.Status == TransactionStatus.Completed && x.FinesPaid.Any());
        if (filter?.FromDate is not null)
            q = q.Where(x => x.TxDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TxDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        var txs = await q.ToListAsync(ct);
        return txs.SelectMany(t => t.FinesPaid.Select(f =>
            new FineCollectedRowDto(t.Id, t.TxNo, t.TxDate, t.StudentId, f.FeeDefinitionId, f.FineNo, f.Amount)))
            .OrderByDescending(x => x.TxDate).ToList();
    }

    public async Task<IReadOnlyList<LatePaymentRowDto>> GetLatePaymentAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var fees = await ActiveFeesQuery().Include(x => x.Installments).ToListAsync(ct);
        var paid = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.InstallmentsPaid.Select(i => new
            {
                x.StudentId,
                i.FeeDefinitionId,
                i.InstallmentNo,
                i.Amount,
                x.TxDate
            }))
            .ToListAsync(ct);

        var rows = new List<LatePaymentRowDto>();
        foreach (var p in paid)
        {
            if (!p.FeeDefinitionId.HasValue) continue;
            var fee = fees.FirstOrDefault(f => f.Id == p.FeeDefinitionId.Value);
            var inst = fee?.Installments.FirstOrDefault(i => i.InstallmentNo == p.InstallmentNo);
            if (inst?.DueDate is null) continue;
            var paidDate = DateOnly.FromDateTime(p.TxDate.UtcDateTime);
            if (paidDate <= inst.DueDate.Value) continue;
            var daysLate = paidDate.DayNumber - inst.DueDate.Value.DayNumber;
            rows.Add(new LatePaymentRowDto(
                p.StudentId, fee!.Id, fee.Name, inst.InstallmentNo,
                inst.DueDate, p.TxDate, daysLate, p.Amount));
        }
        return rows.OrderByDescending(r => r.DaysLate).ToList();
    }

    public async Task<IReadOnlyList<TimelinePointDto>> GetTimeLineAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var datewise = await GetDatewiseFeeCollectionAsync(from, to, ct);
        var expected = await GetFeesExpectedAsync(filter, ct);
        var expectedTotal = expected.Sum(e => e.ExpectedAmount);

        decimal cumulative = 0;
        return datewise.Rows
            .GroupBy(r => r.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var day = Round(g.Sum(x => x.Amount));
                cumulative += day;
                return new TimelinePointDto(g.Key, day, expectedTotal, Round(cumulative));
            }).ToList();
    }

    // ===================== Balances =====================

    public async Task<IReadOnlyList<AdjustmentBalanceRowDto>> GetAdjustmentBalanceAsync(
        string? courseId, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.Adjustments)
            .Where(x => x.Status == TransactionStatus.Completed);

        var txs = await q.ToListAsync(ct);
        return txs.GroupBy(t => t.StudentId)
            .Select(g =>
            {
                var adj = g.SelectMany(t => t.Adjustments).Sum(a => a.Amount);
                var paid = g.Sum(t => t.Amount);
                return new AdjustmentBalanceRowDto(g.Key, Round(adj), Round(paid), Round(paid + adj));
            })
            .Where(x => x.AdjustmentTotal != 0)
            .OrderBy(x => x.StudentId)
            .ToList();
    }

    public async Task<IReadOnlyList<AdvanceBalanceRowDto>> GetAdvanceBalanceAsync(
        IReadOnlyList<Guid>? feeIds, CancellationToken ct)
    {
        var fees = await ActiveFeesQuery().Include(x => x.Students).ToListAsync(ct);
        if (feeIds is { Count: > 0 })
            fees = fees.Where(f => feeIds.Contains(f.Id)).ToList();

        var paid = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.InstallmentsPaid.Select(i => new
            {
                x.StudentId,
                i.FeeDefinitionId,
                i.Amount
            }))
            .ToListAsync(ct);

        var rows = new List<AdvanceBalanceRowDto>();
        var studentIds = fees.SelectMany(f => f.Students.Select(s => s.StudentId))
            .Union(paid.Select(p => p.StudentId))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var sid in studentIds)
        {
            foreach (var fee in fees)
            {
                var applicable = !fee.CollectStudentWise
                                 || fee.Students.Any(s => string.Equals(s.StudentId, sid, StringComparison.OrdinalIgnoreCase));
                if (!applicable) continue;
                var charged = fee.Amount;
                var amountPaid = paid
                    .Where(p => string.Equals(p.StudentId, sid, StringComparison.OrdinalIgnoreCase)
                                && p.FeeDefinitionId == fee.Id)
                    .Sum(p => p.Amount);
                var advance = amountPaid - charged;
                if (advance <= 0) continue;
                rows.Add(new AdvanceBalanceRowDto(sid, fee.Id, fee.Name, charged, Round(amountPaid), Round(advance)));
            }
        }
        return rows.OrderBy(r => r.StudentId).ThenBy(r => r.FeeName).ToList();
    }

    // ===================== History / special =====================

    public async Task<IReadOnlyList<TxHistoryRowDto>> GetTxHistoryAsync(CancellationToken ct)
    {
        return await db.FeeTransactions.AsNoTracking()
            .OrderByDescending(x => x.ModifiedOn)
            .Select(x => new TxHistoryRowDto(
                x.Id, x.TxNo, x.TxDate, x.StudentId, x.PaymentMode, x.Amount,
                x.Status.ToString(), x.CreatedOn, x.CreatedBy, x.ModifiedOn, x.ModifiedBy))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FeesPaidAfterAyRowDto>> GetFeesPaidAfterAcademicYearAsync(
        int? academicYearFrom, CancellationToken ct)
    {
        // Without Acad module, treat AcademicYearFrom as calendar year boundary Jan 1 of that year
        var year = academicYearFrom ?? DateTime.UtcNow.Year;
        var cutoff = new DateTimeOffset(year + 1, 4, 1, 0, 0, 0, TimeSpan.Zero); // typical AY ends ~31 Mar
        return await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed && x.TxDate >= cutoff)
            .OrderBy(x => x.TxDate)
            .Select(x => new FeesPaidAfterAyRowDto(x.Id, x.TxNo, x.TxDate, x.StudentId, x.Amount, year))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FeesPaidByTcRowDto>> GetFeesPaidByTcStudentsAsync(CancellationToken ct)
    {
        // Prefer Stu directory TC tags; fall back to fee Tags containing TC/transfer
        var dir = await students.ListAsync(includeRte: true, includeTc: true, ct: ct);
        var tcFromDir = dir.Where(s => s.IsTc).Select(s => s.StudentId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allFeesWithStudents = await db.Fees.AsNoTracking()
            .Include(f => f.Students)
            .ToListAsync(ct);
        var tcFromFees = allFeesWithStudents
            .Where(f => f.Tags.Any(t => t.Equals("tc", StringComparison.OrdinalIgnoreCase)
                                     || t.Equals("transfer", StringComparison.OrdinalIgnoreCase)))
            .SelectMany(f => f.Students.Select(s => s.StudentId));

        var tcIds = tcFromDir.Union(tcFromFees, StringComparer.OrdinalIgnoreCase).ToList();
        if (tcIds.Count == 0)
            return Array.Empty<FeesPaidByTcRowDto>();

        var txs = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed && tcIds.Contains(x.StudentId))
            .OrderByDescending(x => x.TxDate)
            .ToListAsync(ct);

        var nameMap = dir.ToDictionary(d => d.StudentId, d => d.Name, StringComparer.OrdinalIgnoreCase);
        return txs.Select(t => new FeesPaidByTcRowDto(
            t.StudentId, t.Id, t.TxNo, t.TxDate, t.Amount,
            nameMap.TryGetValue(t.StudentId, out var n) && n is not null
                ? $"TC student: {n}"
                : "TC student")).ToList();
    }

    // ===================== Widgets / JSON =====================

    public async Task<CoursewiseWidgetDto> GetCoursewiseFeeBreakupAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var rows = await GetCoursewiseFeeStructureAsync(filter?.CourseId, ct);
        var courses = rows.Select(r => r.CourseId).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(x => x).ToList();
        var fees = rows.Select(r => r.FeeName).Distinct().OrderBy(x => x).ToList();
        var cells = rows.Select(r => new CoursewiseWidgetCellDto(r.CourseId, r.FeeName, r.Amount)).ToList();
        return new CoursewiseWidgetDto(courses, fees, cells, Round(cells.Sum(c => c.Amount)));
    }

    public async Task<CoursewiseWidgetDto> GetCoursewiseFeeCollectionWidgetAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var paid = await GetFeesPaidAsync(from, to, filter?.CourseId, null, ct);
        var courses = paid.Rows.Select(r => r.CourseId ?? "").Distinct().OrderBy(x => x).ToList();
        var fees = paid.Rows.Select(r => r.FeeName).Distinct().OrderBy(x => x).ToList();
        var cells = paid.Rows
            .GroupBy(r => new { Course = r.CourseId ?? "", r.FeeName })
            .Select(g => new CoursewiseWidgetCellDto(g.Key.Course, g.Key.FeeName, Round(g.Sum(x => x.AmountPaid))))
            .ToList();
        return new CoursewiseWidgetDto(courses, fees, cells, Round(cells.Sum(c => c.Amount)));
    }

    public async Task<IReadOnlyList<PaymentModeBreakupRowDto>> GetPaymentModeBreakupAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed);
        if (filter?.FromDate is not null)
            q = q.Where(x => x.TxDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TxDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        return await q.GroupBy(x => x.PaymentMode ?? "Unknown")
            .Select(g => new PaymentModeBreakupRowDto(g.Key, g.Sum(x => x.Amount), g.LongCount()))
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);
    }

    public async Task<FeeTxHtmlDto?> GetPrintFeeReceiptAsync(Guid txId, CancellationToken ct)
    {
        var tx = await db.FeeTransactions.AsNoTracking()
            .Include(x => x.InstallmentsPaid)
            .Include(x => x.FinesPaid)
            .Include(x => x.Discounts)
            .Include(x => x.Adjustments)
            .SingleOrDefaultAsync(x => x.Id == txId, ct);
        if (tx is null) return null;

        var sb = new StringBuilder();
        sb.Append("<html><body>");
        sb.Append("<h1>Fee Receipt</h1>");
        sb.Append($"<p><b>Receipt No:</b> {System.Net.WebUtility.HtmlEncode(tx.TxNo)}</p>");
        sb.Append($"<p><b>Date:</b> {tx.TxDate:yyyy-MM-dd}</p>");
        sb.Append($"<p><b>Student:</b> {System.Net.WebUtility.HtmlEncode(tx.StudentId)}</p>");
        sb.Append($"<p><b>Payment Mode:</b> {System.Net.WebUtility.HtmlEncode(tx.PaymentMode ?? "")}</p>");
        sb.Append($"<p><b>Ref:</b> {System.Net.WebUtility.HtmlEncode(tx.RefNo ?? "")}</p>");
        sb.Append("<table border='1' cellpadding='4'><tr><th>Type</th><th>No</th><th>Amount</th></tr>");
        foreach (var i in tx.InstallmentsPaid)
            sb.Append($"<tr><td>Installment</td><td>{i.InstallmentNo}</td><td>{i.Amount:N2}</td></tr>");
        foreach (var f in tx.FinesPaid)
            sb.Append($"<tr><td>Fine</td><td>{f.FineNo}</td><td>{f.Amount:N2}</td></tr>");
        foreach (var d in tx.Discounts)
            sb.Append($"<tr><td>Discount</td><td>{System.Net.WebUtility.HtmlEncode(d.Reason ?? "")}</td><td>-{d.Amount:N2}</td></tr>");
        foreach (var a in tx.Adjustments)
            sb.Append($"<tr><td>Adjustment</td><td>{System.Net.WebUtility.HtmlEncode(a.Reason ?? "")}</td><td>{a.Amount:N2}</td></tr>");
        sb.Append($"<tr><td colspan='2'><b>Total</b></td><td><b>{tx.Amount:N2}</b></td></tr>");
        sb.Append("</table></body></html>");
        return new FeeTxHtmlDto(tx.Id, tx.TxNo, sb.ToString());
    }

    public async Task<IReadOnlyList<FeeToPayDto>> GetFeesPayableAsync(string studentId, CancellationToken ct)
    {
        // Reuse same logic as fees-to-be-paid
        var fees = await ActiveFeesQuery()
            .Include(x => x.Installments).Include(x => x.Fines).Include(x => x.Students)
            .Where(x => !x.CollectStudentWise || x.Students.Any(s => s.StudentId == studentId))
            .ToListAsync(ct);

        var paidByFee = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.StudentId == studentId && x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.InstallmentsPaid)
            .GroupBy(x => x.FeeDefinitionId)
            .Select(g => new { FeeId = g.Key, Paid = g.Sum(x => x.Amount) })
            .ToListAsync(ct);
        var paidMap = paidByFee.Where(x => x.FeeId.HasValue)
            .ToDictionary(x => x.FeeId!.Value, x => x.Paid);

        return fees.Select(f =>
        {
            var paid = paidMap.GetValueOrDefault(f.Id, 0m);
            var balance = f.Amount - paid;
            return new FeeToPayDto(
                f.Id, f.Name, f.Amount, paid, balance,
                balance > 0
                    ? f.Installments.OrderBy(i => i.InstallmentNo)
                        .Select(i => new FeeInstallmentDto(i.InstallmentNo, i.Description, i.Amount, i.DueDate)).ToList()
                    : Array.Empty<FeeInstallmentDto>(),
                balance > 0
                    ? f.Fines.OrderBy(i => i.FineNo)
                        .Select(i => new FeeFineDto(i.FineNo, i.Description, i.Amount, i.OverdueByDays)).ToList()
                    : Array.Empty<FeeFineDto>());
        }).Where(x => x.Balance > 0).ToList();
    }

    public async Task<IReadOnlyList<FeeRioAccountRowDto>> GetFeeReceivedInOtherAccountAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed
                        && x.ReceivedInAccount != null && x.ReceivedInAccount != "");
        if (filter?.FromDate is not null)
            q = q.Where(x => x.TxDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TxDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        return await q.OrderByDescending(x => x.TxDate)
            .Select(x => new FeeRioAccountRowDto(
                x.Id, x.TxNo, x.TxDate, x.StudentId, x.ReceivedInAccount!, x.Amount))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConcessionRowDto>> GetConcessionAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking()
            .Include(x => x.Discounts)
            .Where(x => x.Discounts.Any());
        if (filter?.FromDate is not null)
            q = q.Where(x => x.TxDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TxDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(filter?.StudentId))
            q = q.Where(x => x.StudentId == filter.StudentId);

        var txs = await q.ToListAsync(ct);
        return txs.SelectMany(t => t.Discounts.Select(d =>
            new ConcessionRowDto(t.Id, t.TxNo, t.TxDate, t.StudentId, d.Reason, d.Amount)))
            .OrderByDescending(x => x.TxDate).ToList();
    }

    public async Task<IReadOnlyList<RefundReportRowDto>> GetRefundReportAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.Refunds.AsNoTracking().AsQueryable();
        if (filter?.FromDate is not null)
            q = q.Where(x => x.RefundDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.RefundDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(filter?.StudentId))
            q = q.Where(x => x.StudentId == filter.StudentId);

        return await q.OrderByDescending(x => x.RefundDate)
            .Select(x => new RefundReportRowDto(
                x.Id, x.StudentId, x.FeeTransactionId, x.RefundDate, x.Amount,
                x.Status.ToString(), x.Reason, x.Reference))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuspenseReportRowDto>> GetSuspenseAccountTxnReportAsync(
        ReportFilterRequest? filter, CancellationToken ct)
    {
        var q = db.SuspenseAccountTransactions.AsNoTracking()
            .Include(x => x.Allocations)
            .AsQueryable();
        if (filter?.FromDate is not null)
            q = q.Where(x => x.TransactionDate >= filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (filter?.ToDate is not null)
            q = q.Where(x => x.TransactionDate <= filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        var list = await q.OrderByDescending(x => x.TransactionDate).ToListAsync(ct);
        return list.Select(s =>
        {
            var allocated = s.Allocations.Sum(a => a.AllocatedAmount);
            return new SuspenseReportRowDto(
                s.Id, s.TransactionDate, s.Amount, Round(allocated),
                Round(s.Amount - allocated), s.Status.ToString(), s.PaymentMode, s.Particulars);
        }).ToList();
    }

    // ===================== Helpers =====================

    private IQueryable<FeeDefinition> ActiveFeesQuery() =>
        db.Fees.AsNoTracking().Where(x => x.Status == FeeStatus.Active);

    private static ReceiptTransactionRowDto MapReceipt(FeeTransaction tx) => new(
        tx.Id, tx.TxNo, tx.TxDate, tx.StudentId, tx.PaymentMode, tx.RefNo, tx.ReceivedInAccount,
        tx.Amount, tx.Status.ToString(), tx.IsOpeningBalanceAdjusted,
        tx.InstallmentsPaid.Select(i => new TxLineDetailDto(i.FeeDefinitionId, i.InstallmentNo, i.Amount)).ToList(),
        tx.FinesPaid.Select(i => new TxLineDetailDto(i.FeeDefinitionId, i.FineNo, i.Amount)).ToList(),
        tx.Discounts.Select(d => new NamedAmountDto(d.Reason, d.Amount)).ToList(),
        tx.Adjustments.Select(a => new NamedAmountDto(a.Reason, a.Amount)).ToList());

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static decimal Round(decimal v) => decimal.Round(v, 2, MidpointRounding.AwayFromZero);
}
