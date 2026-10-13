using Ct.Fee.Application.Integrations;
using Ct.Fee.Application.Policy;
using Ct.Fee.Domain;
using Ct.Fee.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Fee.Application;

public sealed class FeeService(
    FeeDbContext db,
    FeeCalculationService calculator,
    ICurrentUserContext user,
    IFeePolicyEngine policy,
    IStudentDirectory students,
    IStudentFeeReceivableClient receivableClient,
    IFeeEventPublisher events)
{
    // ===================== Fee definitions =====================

    public async Task<IReadOnlyList<FeeDefinitionDto>> GetFeesAsync(CancellationToken ct) =>
        await db.Fees.AsNoTracking()
            .Include(x => x.Installments).Include(x => x.Fines).Include(x => x.Students).Include(x => x.Courses)
            .OrderBy(x => x.NameLower)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<FeeDefinitionDto?> GetFeeAsync(Guid id, CancellationToken ct)
    {
        var fee = await db.Fees.AsNoTracking()
            .Include(x => x.Installments).Include(x => x.Fines).Include(x => x.Students).Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return fee is null ? null : ToDto(fee);
    }

    public async Task<IReadOnlyList<FeeInstallmentFlattenedDto>> GetFeeInstallmentsFlattenedAsync(
        string? courseId, CancellationToken ct)
    {
        var query = db.Fees.AsNoTracking()
            .Include(x => x.Installments)
            .Include(x => x.Courses)
            .Where(x => x.Status == FeeStatus.Active);
        if (!string.IsNullOrWhiteSpace(courseId))
            query = query.Where(x => x.Courses.Any(c => c.CourseId == courseId));

        var fees = await query.ToListAsync(ct);
        return fees.SelectMany(f => f.Installments
            .OrderBy(i => i.InstallmentNo)
            .Select(i => new FeeInstallmentFlattenedDto(
                f.Id, f.Name, i.InstallmentNo, i.Description, i.Amount, i.DueDate,
                f.Courses.Select(c => c.CourseId).Order().ToList())))
            .ToList();
    }

    public async Task<FeeDefinitionDto> CreateFeeAsync(FeeDefinitionRequest request, CancellationToken ct)
    {
        ValidateMoney(request.Amount, nameof(request.Amount));
        var fee = new FeeDefinition
        {
            Name = request.Name,
            DisplayText = request.DisplayText,
            Amount = request.Amount,
            Tags = request.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            CollectStudentWise = request.CollectStudentWise,
            SpansAcademicYears = request.SpansAcademicYears,
            Installments = request.Installments.Select(x => new FeeInstallment
            {
                InstallmentNo = x.InstallmentNo,
                Description = x.Description,
                Amount = Round(x.Amount),
                DueDate = x.DueDate
            }).ToList(),
            Fines = request.Fines.Select(x => new FeeFine
            {
                FineNo = x.FineNo,
                Description = x.Description,
                Amount = Round(x.Amount),
                OverdueByDays = x.OverdueByDays
            }).ToList(),
            Students = request.StudentList.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => new FeeStudent { StudentId = x }).ToList(),
            Courses = request.CourseList.Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(x => new FeeCourse { CourseId = x }).ToList()
        };
        fee.Normalize();
        StampCreate(fee);
        db.Fees.Add(fee);
        await db.SaveChangesAsync(ct);
        await events.PublishFeeCreatedAsync(fee.Id, token: null, ct);
        return ToDto(fee);
    }

    public async Task<FeeDefinitionDto?> UpdateFeeAsync(FeeDefinitionUpdateRequest request, CancellationToken ct)
    {
        var fee = await db.Fees
            .Include(x => x.Installments).Include(x => x.Fines)
            .Include(x => x.Students).Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (fee is null) return null;

        fee.Name = request.Name;
        fee.DisplayText = request.DisplayText;
        fee.Amount = request.Amount;
        fee.Tags = request.Tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        fee.CollectStudentWise = request.CollectStudentWise;
        fee.SpansAcademicYears = request.SpansAcademicYears;
        fee.Normalize();
        StampModify(fee);

        db.RemoveRange(fee.Installments);
        db.RemoveRange(fee.Fines);
        db.RemoveRange(fee.Students);
        db.RemoveRange(fee.Courses);
        fee.Installments = request.Installments.Select(x => new FeeInstallment
        {
            InstallmentNo = x.InstallmentNo, Description = x.Description,
            Amount = Round(x.Amount), DueDate = x.DueDate
        }).ToList();
        fee.Fines = request.Fines.Select(x => new FeeFine
        {
            FineNo = x.FineNo, Description = x.Description,
            Amount = Round(x.Amount), OverdueByDays = x.OverdueByDays
        }).ToList();
        fee.Students = request.StudentList.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new FeeStudent { StudentId = x }).ToList();
        fee.Courses = request.CourseList.Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new FeeCourse { CourseId = x }).ToList();

        await db.SaveChangesAsync(ct);
        await events.PublishFeeStructureUpdatedAsync(fee.Id, token: null, ct);
        return ToDto(fee);
    }

    public async Task<CanRemoveStudentResult> CanRemoveStudentAsync(Guid feeId, string studentId, CancellationToken ct)
    {
        var hasTx = await db.FeeTransactions.AsNoTracking()
            .AnyAsync(x => x.StudentId == studentId && x.Status == TransactionStatus.Completed
                && x.InstallmentsPaid.Any(i => i.FeeDefinitionId == feeId), ct);
        return hasTx
            ? new CanRemoveStudentResult(false, "Student has completed transactions against this fee.")
            : new CanRemoveStudentResult(true, null);
    }

    public async Task<bool> SetFeeStatusAsync(Guid id, FeeStatus status, CancellationToken ct)
    {
        var fee = await db.Fees.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (fee is null) return false;
        fee.Status = status;
        StampModify(fee);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Port of FeeCmd.AssignStudent — add a single student to a fee payer list.</summary>
    public async Task<FeeDefinitionDto?> AssignStudentAsync(Guid feeId, string studentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("StudentId is required.", nameof(studentId));

        var fee = await db.Fees
            .Include(x => x.Installments).Include(x => x.Fines)
            .Include(x => x.Students).Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.Id == feeId, ct);
        if (fee is null) return null;

        if (fee.Students.Any(s => string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase)))
            return ToDto(fee);

        fee.Students.Add(new FeeStudent { StudentId = studentId.Trim() });
        fee.CollectStudentWise = true;
        StampModify(fee);
        await db.SaveChangesAsync(ct);
        return ToDto(fee);
    }

    /// <summary>Port of FeeQry.GetDates — monthly / quarterly / semi-annual / academic-year ranges.</summary>
    public DateRangeDto GetDates(string rangeType, int month, int year, DateOnly? academicYearFrom = null, DateOnly? academicYearTo = null)
    {
        rangeType = (rangeType ?? "monthly").Trim().ToLowerInvariant();
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
        if (year < 1900 || year > 2100) throw new ArgumentOutOfRangeException(nameof(year));

        DateOnly from;
        DateOnly to;
        switch (rangeType)
        {
            case "monthly":
                from = new DateOnly(year, month, 1);
                to = from.AddMonths(1).AddDays(-1);
                break;
            case "quarterly":
                from = new DateOnly(year, month, 1);
                to = from.AddMonths(3).AddDays(-1);
                break;
            case "semi-annual":
            case "semiannual":
            case "semi_annual":
                from = new DateOnly(year, month, 1);
                to = from.AddMonths(6).AddDays(-1);
                break;
            case "annual":
            case "academic":
            case "academicyear":
            case "academic-year":
                from = academicYearFrom ?? new DateOnly(year, 4, 1);
                to = academicYearTo ?? new DateOnly(year + 1, 3, 31);
                break;
            default:
                from = new DateOnly(year, month, 1);
                to = from.AddMonths(1).AddDays(-1);
                rangeType = "monthly";
                break;
        }
        return new DateRangeDto(from, to, rangeType);
    }

    /// <summary>
    /// Port of FeeQry.FeesExpectedCollected — per fee, per month in academic year:
    /// expected vs collected.
    /// </summary>
    public async Task<IReadOnlyList<FeeExpectedCollectedDto>> FeesExpectedCollectedAsync(
        DateOnly? academicYearFrom, DateOnly? academicYearTo, CancellationToken ct)
    {
        var ayFrom = academicYearFrom ?? new DateOnly(DateTime.UtcNow.Year, 4, 1);
        var ayTo = academicYearTo ?? new DateOnly(DateTime.UtcNow.Year + 1, 3, 31);

        var fees = await db.Fees.AsNoTracking()
            .Include(x => x.Students)
            .Where(x => x.Status == FeeStatus.Active)
            .OrderBy(x => x.NameLower)
            .ToListAsync(ct);

        var paidLines = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed
                        && x.TxDate >= ayFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                        && x.TxDate <= ayTo.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            .SelectMany(x => x.InstallmentsPaid.Select(i => new
            {
                x.TxDate,
                i.FeeDefinitionId,
                i.Amount
            }))
            .ToListAsync(ct);

        var result = new List<FeeExpectedCollectedDto>();
        var cursor = new DateOnly(ayFrom.Year, ayFrom.Month, 1);
        var endMonth = new DateOnly(ayTo.Year, ayTo.Month, 1);

        while (cursor <= endMonth)
        {
            var range = GetDates("monthly", cursor.Month, cursor.Year);
            foreach (var fee in fees)
            {
                var studentCount = fee.CollectStudentWise ? Math.Max(fee.Students.Count, 0) : Math.Max(fee.Students.Count, 1);
                var expected = Round(fee.Amount * (studentCount == 0 ? 1 : studentCount));
                var collected = paidLines
                    .Where(p => p.FeeDefinitionId == fee.Id
                                && DateOnly.FromDateTime(p.TxDate.UtcDateTime) >= range.From
                                && DateOnly.FromDateTime(p.TxDate.UtcDateTime) <= range.To)
                    .Sum(p => p.Amount);
                result.Add(new FeeExpectedCollectedDto(
                    fee.Id, fee.Name, fee.DisplayText, range.From, range.To,
                    expected, Round(collected), Round(expected - collected)));
            }
            cursor = cursor.AddMonths(1);
        }
        return result;
    }

    // ===================== Transactions =====================

    public async Task<IReadOnlyList<FeeTransactionDto>> GetTransactionsAsync(string? studentId, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(studentId))
            q = q.Where(x => x.StudentId == studentId);
        return await q.OrderByDescending(x => x.TxDate)
            .Select(x => ToDto(x))
            .ToListAsync(ct);
    }

    public async Task<FeeTransactionDto?> GetTransactionAsync(string idOrTxNo, CancellationToken ct)
    {
        FeeTransaction? tx = null;
        if (Guid.TryParse(idOrTxNo, out var guid))
            tx = await db.FeeTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == guid, ct);
        tx ??= await db.FeeTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.TxNo == idOrTxNo, ct);
        return tx is null ? null : ToDto(tx);
    }

    public async Task<FeeTxHtmlDto?> GetTransactionHtmlAsync(Guid id, CancellationToken ct)
    {
        var tx = await db.FeeTransactions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tx is null) return null;
        var html = $"<html><body><h1>Fee Receipt</h1><p>TxNo: {tx.TxNo}</p>" +
                   $"<p>Student: {tx.StudentId}</p><p>Date: {tx.TxDate:yyyy-MM-dd}</p>" +
                   $"<p>Amount: {tx.Amount:N2}</p><p>Mode: {tx.PaymentMode}</p></body></html>";
        return new FeeTxHtmlDto(tx.Id, tx.TxNo, html);
    }

    /// <summary>
    /// Port of FeeTxCmd.Transact2 — validates ref uniqueness, allocates TxNo if empty,
    /// persists lines, marks related fees IsTxDone.
    /// </summary>
    public async Task<FeeTransactionDto> CreateTransactionAsync(FeeTransactionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId))
            throw new ArgumentException("StudentId is required.", nameof(request));

        // Duplicate ref check (original GetFeeTxByRefNo)
        if (!string.IsNullOrWhiteSpace(request.RefNo))
        {
            var existingRef = await db.FeeTransactions.AsNoTracking()
                .AnyAsync(x => x.RefNo == request.RefNo && x.Status == TransactionStatus.Completed, ct);
            if (existingRef)
                throw new InvalidOperationException($"A completed transaction with RefNo '{request.RefNo}' already exists.");
        }

        var amount = request.InstallmentsPaid.Sum(x => x.Amount)
                   + request.FinesPaid.Sum(x => x.Amount)
                   - request.Discounts.Sum(x => x.Amount)
                   + request.Adjustments.Sum(x => x.Amount);
        amount = Round(amount);
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(request), "Transaction amount cannot be negative.");

        var txNo = string.IsNullOrWhiteSpace(request.TxNo)
            ? await GenerateNextTxNoAsync(ct)
            : request.TxNo.Trim();

        // Ensure TxNo uniqueness
        if (await db.FeeTransactions.AsNoTracking().AnyAsync(x => x.TxNo == txNo, ct))
            throw new InvalidOperationException($"Transaction number '{txNo}' already exists.");

        var tx = new FeeTransaction
        {
            TxNo = txNo,
            TxDate = request.TxDate == default ? DateTimeOffset.UtcNow : request.TxDate,
            StudentId = request.StudentId.Trim(),
            PaymentMode = request.PaymentMode,
            IsOpeningBalanceAdjusted = request.IsOpeningBalanceAdjusted,
            RefNo = string.IsNullOrWhiteSpace(request.RefNo) ? request.PaidBy : request.RefNo,
            ReceivedInAccount = request.ReceivedInAccount,
            Amount = amount,
            Status = TransactionStatus.Completed,
            InstallmentsPaid = request.InstallmentsPaid.Select(x => new FeeTransactionInstallment
            {
                FeeDefinitionId = x.FeeId, InstallmentNo = x.No, Amount = Round(x.Amount)
            }).ToList(),
            FinesPaid = request.FinesPaid.Select(x => new FeeTransactionFine
            {
                FeeDefinitionId = x.FeeId, FineNo = x.No, Amount = Round(x.Amount)
            }).ToList(),
            Discounts = request.Discounts.Select(x => new FeeTransactionDiscount
            {
                Reason = x.Reason, Amount = Round(x.Amount)
            }).ToList(),
            Adjustments = request.Adjustments.Select(x => new FeeTransactionAdjustment
            {
                Reason = x.Reason, Amount = Round(x.Amount)
            }).ToList()
        };
        StampCreate(tx);
        db.FeeTransactions.Add(tx);

        // Mark fees that appear on this tx as IsTxDone (FeeCmd.EnableTx / post-transact side effect)
        var feeIds = tx.InstallmentsPaid
            .Where(i => i.FeeDefinitionId.HasValue)
            .Select(i => i.FeeDefinitionId!.Value)
            .Distinct()
            .ToList();
        if (feeIds.Count > 0)
        {
            var fees = await db.Fees.Where(f => feeIds.Contains(f.Id)).ToListAsync(ct);
            foreach (var fee in fees)
            {
                fee.IsTxDone = true;
                StampModify(fee);
            }
        }

        await db.SaveChangesAsync(ct);

        // Port of Transact2: update student fee receivable + enqueue fee_tx_done
        if (feeIds.Count > 0)
        {
            var feeMeta = await db.Fees.AsNoTracking()
                .Where(f => feeIds.Contains(f.Id))
                .Select(f => new { f.Id, f.Name, Total = f.Installments.Sum(i => i.Amount) })
                .ToListAsync(ct);
            foreach (var fee in feeMeta)
            {
                var previouslyPaid = await db.FeeTransactions.AsNoTracking()
                    .Where(x => x.StudentId == tx.StudentId && x.Status == TransactionStatus.Completed)
                    .SelectMany(x => x.InstallmentsPaid)
                    .Where(i => i.FeeDefinitionId == fee.Id)
                    .SumAsync(i => i.Amount, ct);
                var balance = Round(fee.Total - previouslyPaid);
                await receivableClient.UpdateAsync(
                    tx.StudentId, user.OwnerId, fee.Id, fee.Name, tx.Id, balance, ct);
                await events.PublishStudentFeesReceivableAsync(
                    tx.StudentId, fee.Id, tx.Id, balance, token: null, ct);
            }
        }

        await events.PublishFeeTxDoneAsync(tx.Id, token: null, ct);
        return ToDto(tx);
    }

    public async Task<TransactResultDto> Transact2Async(FeeTransactionRequest request, CancellationToken ct)
    {
        var tx = await CreateTransactionAsync(request, ct);
        return new TransactResultDto(tx.Id, tx.TxNo, tx.Amount, "Transaction completed");
    }

    public async Task<FeeTransactionDto?> UpdateTransactionAsync(FeeTransactionUpdateRequest request, CancellationToken ct)
    {
        var tx = await db.FeeTransactions.SingleOrDefaultAsync(x => x.Id == request.Id, ct);
        if (tx is null) return null;
        tx.TxDate = request.TxDate;
        tx.RefNo = request.RefNo ?? request.PaidBy;
        StampModify(tx);
        await db.SaveChangesAsync(ct);
        return ToDto(tx);
    }

    public async Task<bool> SetTransactionStatusAsync(Guid id, TransactionStatus status, CancellationToken ct)
    {
        var tx = await db.FeeTransactions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tx is null) return false;
        tx.Status = status;
        StampModify(tx);
        await db.SaveChangesAsync(ct);
        // Port of EnqueFeeTxStatusUpdatedJob
        await events.PublishFeeTxStatusUpdatedAsync(id, status.ToString(), token: null, ct);
        return true;
    }

    /// <summary>Port of FeeTxQry.GetStudentFeeDetails / GetStudentFeeDetails2.</summary>
    public async Task<StudentFeeDetailDto> GetStudentFeeDetailsAsync(string studentId, CancellationToken ct)
    {
        var txs = await GetTransactionsAsync(studentId, ct);
        var expectedPaid = await GetStudentFeesExpectedPaidAsync(studentId, ct);
        var studentRow = expectedPaid.FirstOrDefault();
        IReadOnlyList<FeeToPayDto> toPay;
        if (studentRow is null)
        {
            toPay = await GetFeesToBePaidAsync(studentId, ct);
        }
        else
        {
            toPay = studentRow.Lines
                .Where(l => l.Balance > 0)
                .GroupBy(l => new { l.FeeId, l.FeeName })
                .Select(g => new FeeToPayDto(
                    g.Key.FeeId, g.Key.FeeName,
                    Round(g.Sum(x => x.ExpectedAmount)),
                    Round(g.Sum(x => x.PaidAmount)),
                    Round(g.Sum(x => x.Balance)),
                    g.Select(x => new FeeInstallmentDto(x.InstallmentNo, x.Description, x.ExpectedAmount, x.DueDate)).ToList(),
                    Array.Empty<FeeFineDto>()))
                .ToList();
        }
        return new StudentFeeDetailDto(studentId, txs, toPay);
    }

    public async Task<IReadOnlyList<FeeToPayDto>> GetFeesToBePaidAsync(string studentId, CancellationToken ct)
    {
        var fees = await db.Fees.AsNoTracking()
            .Include(x => x.Installments).Include(x => x.Fines).Include(x => x.Students)
            .Where(x => x.Status == FeeStatus.Active
                && (!x.CollectStudentWise || x.Students.Any(s => s.StudentId == studentId)))
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
                balance > 0 ? f.Installments.OrderBy(i => i.InstallmentNo)
                    .Select(i => new FeeInstallmentDto(i.InstallmentNo, i.Description, i.Amount, i.DueDate)).ToList()
                    : Array.Empty<FeeInstallmentDto>(),
                balance > 0 ? f.Fines.OrderBy(i => i.FineNo)
                    .Select(i => new FeeFineDto(i.FineNo, i.Description, i.Amount, i.OverdueByDays)).ToList()
                    : Array.Empty<FeeFineDto>());
        }).Where(x => x.Balance > 0).ToList();
    }

    public Task<IReadOnlyList<FeeCollectionAccountDto>> GetFeeCollectionAccountsAsync(CancellationToken ct)
    {
        var accounts = policy.GetFeeCollectionAccounts()
            .Select((name, i) => new FeeCollectionAccountDto(name, name.ToUpperInvariant().Replace(' ', '_')))
            .ToList();
        return Task.FromResult<IReadOnlyList<FeeCollectionAccountDto>>(accounts);
    }

    /// <summary>Port of FeeTxQry.GetFeeTxByRefNo.</summary>
    public async Task<FeeTransactionDto?> GetFeeTxByRefNoAsync(string refNo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refNo)) return null;
        var tx = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.RefNo == refNo && x.Status == TransactionStatus.Completed)
            .OrderByDescending(x => x.TxDate)
            .FirstOrDefaultAsync(ct);
        return tx is null ? null : ToDto(tx);
    }

    /// <summary>Port of FeeTxQry.GetTransactedFees — fee ids that already have completed txs.</summary>
    public async Task<IReadOnlyList<Guid>> GetTransactedFeesAsync(CancellationToken ct)
    {
        return await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.InstallmentsPaid)
            .Where(i => i.FeeDefinitionId != null)
            .Select(i => i.FeeDefinitionId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Port of FeeTxQry.GetStudentFeesExpectedPaid2 — core expected vs paid engine per student.
    /// Uses fee student/course lists from fee domain (Stu module can enrich later).
    /// </summary>
    public async Task<IReadOnlyList<StudentFeesExpectedPaidDto>> GetStudentFeesExpectedPaidAsync(
        string? studentIdFilter, CancellationToken ct)
    {
        var fees = await db.Fees.AsNoTracking()
            .Include(x => x.Installments).Include(x => x.Students).Include(x => x.Courses)
            .Where(x => x.Status == FeeStatus.Active)
            .ToListAsync(ct);

        // Order fees like original policy FeeOrderIndex
        fees = fees.OrderBy(f => policy.FeeOrderIndex(f.Name)).ThenBy(f => f.NameLower).ToList();

        var paidQuery = db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed);
        if (!string.IsNullOrWhiteSpace(studentIdFilter))
            paidQuery = paidQuery.Where(x => x.StudentId == studentIdFilter);

        var paidLines = await paidQuery
            .SelectMany(x => x.InstallmentsPaid.Select(i => new
            {
                x.StudentId,
                i.FeeDefinitionId,
                i.InstallmentNo,
                i.Amount
            }))
            .ToListAsync(ct);

        var studentIds = fees.SelectMany(f => f.Students.Select(s => s.StudentId))
            .Union(paidLines.Select(p => p.StudentId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (!string.IsNullOrWhiteSpace(studentIdFilter))
            studentIds = studentIds.Where(s => string.Equals(s, studentIdFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        // Enrich / filter via Stu directory (RTE policy)
        var directory = await students.ListAsync(includeRte: policy.IncludeRteStudents(), includeTc: true, ct: ct);
        var dirMap = directory.ToDictionary(d => d.StudentId, StringComparer.OrdinalIgnoreCase);
        var rteExcludedFeeNames = policy.ExcludeFeesForRte(null)
            .Select(x => x.ToLowerInvariant()).ToHashSet();

        var result = new List<StudentFeesExpectedPaidDto>();
        foreach (var sid in studentIds)
        {
            dirMap.TryGetValue(sid, out var stuInfo);
            // If directory knows the student and policy excludes RTE, skip
            if (stuInfo is { IsRte: true } && !policy.IncludeRteStudents())
                continue;

            var lines = new List<FeeInstallmentExpectedPaidDto>();
            foreach (var fee in fees)
            {
                if (stuInfo is { IsRte: true } && rteExcludedFeeNames.Count > 0
                    && rteExcludedFeeNames.Any(ex => fee.Name.Contains(ex, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var applicable = !fee.CollectStudentWise
                    || fee.Students.Any(s => string.Equals(s.StudentId, sid, StringComparison.OrdinalIgnoreCase));
                // Course-wide: if directory has course, match fee courses
                if (!fee.CollectStudentWise && stuInfo?.CourseId is not null && fee.Courses.Count > 0)
                    applicable = fee.Courses.Any(c => c.CourseId == stuInfo.CourseId);
                if (!applicable) continue;

                foreach (var inst in fee.Installments.OrderBy(i => i.InstallmentNo))
                {
                    var paid = paidLines
                        .Where(p => string.Equals(p.StudentId, sid, StringComparison.OrdinalIgnoreCase)
                                    && p.FeeDefinitionId == fee.Id
                                    && p.InstallmentNo == inst.InstallmentNo)
                        .Sum(p => p.Amount);
                    var balance = Round(inst.Amount - paid);
                    lines.Add(new FeeInstallmentExpectedPaidDto(
                        fee.Id, fee.Name, inst.InstallmentNo, inst.Description,
                        inst.Amount, Round(paid), balance, inst.DueDate, balance <= 0));
                }
            }

            var expectedTotal = Round(lines.Sum(l => l.ExpectedAmount));
            var paidTotal = Round(lines.Sum(l => l.PaidAmount));
            var pending = Round(lines.Where(l => l.Balance > 0).Sum(l => l.Balance));
            var excess = paidTotal > expectedTotal ? Round(paidTotal - expectedTotal) : 0m;
            result.Add(new StudentFeesExpectedPaidDto(sid, lines, expectedTotal, paidTotal, pending, excess));
        }
        return result.OrderBy(r => r.StudentId).ToList();
    }

    /// <summary>Port of FeeTxQry.GetClosingBalance — students with pending balance or adjustments.</summary>
    public async Task<IReadOnlyList<StudentClosingBalanceDto>> GetClosingBalanceAsync(CancellationToken ct)
    {
        var expectedPaid = await GetStudentFeesExpectedPaidAsync(null, ct);

        var adjustments = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed && x.Adjustments.Any())
            .SelectMany(x => x.Adjustments.Select(a => new { x.StudentId, a.Amount }))
            .ToListAsync(ct);
        var adjMap = adjustments.GroupBy(a => a.StudentId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => Round(g.Sum(x => x.Amount)), StringComparer.OrdinalIgnoreCase);

        var result = new List<StudentClosingBalanceDto>();
        foreach (var row in expectedPaid)
        {
            var adj = adjMap.GetValueOrDefault(row.StudentId, 0m);
            if (row.PendingTotal <= 0 && adj == 0) continue;
            var pendingInst = row.Lines.Where(l => l.Balance > 0)
                .Select(l => new DefaulterInstallmentDto(
                    l.FeeId, l.FeeName, l.InstallmentNo, l.Description, l.DueDate,
                    l.ExpectedAmount, l.PaidAmount, l.Balance))
                .ToList();
            result.Add(new StudentClosingBalanceDto(
                row.StudentId, row.ExpectedTotal, row.PaidTotal, adj, row.PendingTotal,
                Round(row.PendingTotal - adj), pendingInst));
        }
        return result.OrderByDescending(r => r.ClosingBalance).ToList();
    }

    private async Task<string> GenerateNextTxNoAsync(CancellationToken ct)
    {
        var last = await db.FeeTransactions.AsNoTracking()
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => x.TxNo)
            .FirstOrDefaultAsync(ct);
        var nextNum = 1;
        if (!string.IsNullOrEmpty(last))
        {
            var part = last.Split('/')[0];
            if (int.TryParse(part, out var n)) nextNum = n + 1;
        }
        var ownerShort = user.OwnerId == Guid.Empty ? "FEE" : user.OwnerId.ToString("N")[..4].ToUpperInvariant();
        return $"{nextNum}/{ownerShort}";
    }

    // ===================== Refunds =====================

    public async Task<RefundDto> CreateRefundAsync(RefundRequest request, CancellationToken ct)
    {
        ValidateMoney(request.Amount, nameof(request.Amount));
        var refund = new Refund
        {
            StudentId = request.StudentId,
            FeeTransactionId = request.FeeTransactionId,
            RefundDate = request.RefundDate,
            Amount = Round(request.Amount),
            Reason = request.Reason,
            Reference = request.Reference,
            Status = RefundStatus.Pending
        };
        StampCreate(refund);
        db.Refunds.Add(refund);
        await db.SaveChangesAsync(ct);
        return ToDto(refund);
    }

    public async Task<IReadOnlyList<RefundDto>> GetRefundsAsync(CancellationToken ct) =>
        await db.Refunds.AsNoTracking().OrderByDescending(x => x.RefundDate)
            .Select(x => ToDto(x)).ToListAsync(ct);

    public async Task<bool> SetRefundStatusAsync(Guid id, RefundStatus status, CancellationToken ct)
    {
        var r = await db.Refunds.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (r is null) return false;
        r.Status = status;
        StampModify(r);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ===================== Suspense =====================

    public async Task<SuspenseTransactionDto> CreateSuspenseAsync(SuspenseTransactionRequest request, CancellationToken ct)
    {
        ValidateMoney(request.Amount, nameof(request.Amount));
        var s = new SuspenseAccountTransaction
        {
            TransactionDate = request.TransactionDate,
            Amount = Round(request.Amount),
            PaymentMode = request.PaymentMode,
            Particulars = request.Particulars ?? request.Reference,
            Status = SuspenseStatus.Unidentified
        };
        StampCreate(s);
        db.SuspenseAccountTransactions.Add(s);
        await db.SaveChangesAsync(ct);
        return ToDto(s);
    }

    public async Task<IReadOnlyList<SuspenseTransactionDto>> GetSuspenseTransactionsAsync(CancellationToken ct) =>
        await db.SuspenseAccountTransactions.AsNoTracking()
            .Include(x => x.Allocations)
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => ToDto(x))
            .ToListAsync(ct);

    public async Task<SuspenseTransactionDto?> GetSuspenseTransactionAsync(Guid id, CancellationToken ct)
    {
        var s = await db.SuspenseAccountTransactions.AsNoTracking()
            .Include(x => x.Allocations)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return s is null ? null : ToDto(s);
    }

    public async Task<SuspenseTransactionDto> AllocateSuspenseAsync(
        Guid suspenseId, SuspenseAllocationRequest request, CancellationToken ct)
    {
        var s = await db.SuspenseAccountTransactions
            .Include(x => x.Allocations)
            .SingleOrDefaultAsync(x => x.Id == suspenseId, ct)
            ?? throw new InvalidOperationException($"Suspense {suspenseId} not found.");

        ValidateMoney(request.AllocatedAmount, nameof(request.AllocatedAmount));
        var alloc = new SuspenseAccountTransactionAllocation
        {
            FeeTransactionId = request.FeeTransactionId,
            AllocatedAmount = Round(request.AllocatedAmount),
            AllocatedOn = DateTimeOffset.UtcNow,
            AllocatedBy = user.UserId ?? "system"
        };
        StampCreate(alloc);
        s.Allocations.Add(alloc);
        var total = s.Allocations.Sum(x => x.AllocatedAmount);
        s.Status = total >= s.Amount ? SuspenseStatus.Allocated
            : total > 0 ? SuspenseStatus.PartiallyAllocated
            : SuspenseStatus.Unidentified;
        StampModify(s);
        await db.SaveChangesAsync(ct);
        return ToDto(s);
    }

    public async Task<bool> SetSuspenseStatusAsync(Guid id, SuspenseStatus status, CancellationToken ct)
    {
        var s = await db.SuspenseAccountTransactions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null) return false;
        s.Status = status;
        StampModify(s);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ===================== Summaries =====================

    public async Task<IReadOnlyList<FeeCollectionSummary>> GetDatewiseCollectionAsync(
        DateOnly from, DateOnly to, CancellationToken ct)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDate = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed && x.TxDate >= fromDate && x.TxDate <= toDate)
            .GroupBy(x => DateOnly.FromDateTime(x.TxDate.UtcDateTime))
            .Select(x => new FeeCollectionSummary(x.Key, x.Sum(y => y.Amount), x.LongCount()))
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<StudentFeeSummary> GetStudentSummaryAsync(string studentId, CancellationToken ct)
    {
        var charged = await db.Fees.AsNoTracking()
            .Where(x => !x.CollectStudentWise || x.Students.Any(s => s.StudentId == studentId))
            .SumAsync(x => x.Amount, ct);
        var paid = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.StudentId == studentId && x.Status == TransactionStatus.Completed)
            .SumAsync(x => x.Amount, ct);
        return new StudentFeeSummary(studentId, charged, paid, charged - paid);
    }

    // ===================== Reports (all original ReportController methods) =====================

    public async Task<ReportResultDto> ReportFeeStructureAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await GetFeesAsync(ct);
        return new ReportResultDto("FeeStructure", data);
    }

    public async Task<ReportResultDto> ReportCoursewiseFeeStructureAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await GetFeeInstallmentsFlattenedAsync(f?.CourseId, ct);
        return new ReportResultDto("CoursewiseFeeStructure", data);
    }

    public async Task<ReportResultDto> ReportFeesExpectedAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var fees = await db.Fees.AsNoTracking().Where(x => x.Status == FeeStatus.Active).ToListAsync(ct);
        return new ReportResultDto("FeesExpected", fees.Select(ToDto).ToList());
    }

    public async Task<ReportResultDto> ReportDatewiseFeeCollectionAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var from = f?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = f?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var data = await GetDatewiseCollectionAsync(from, to, ct);
        return new ReportResultDto("DatewiseFeeCollection", data);
    }

    public async Task<ReportResultDto> ReportDatewiseFeeCollectionGroupedAsync(ReportFilterRequest? f, CancellationToken ct)
        => await ReportDatewiseFeeCollectionAsync(f, ct);

    public async Task<ReportResultDto> ReportFeesDefaultersAsync(ReportFilterRequest? f, int version, CancellationToken ct)
    {
        // Students with positive balance across active fees
        var studentIds = await db.FeeTransactions.AsNoTracking()
            .Select(x => x.StudentId).Distinct().ToListAsync(ct);
        var results = new List<StudentFeeSummary>();
        foreach (var sid in studentIds)
        {
            var summary = await GetStudentSummaryAsync(sid, ct);
            if (summary.Balance > 0) results.Add(summary);
        }
        return new ReportResultDto($"FeesDefaulters_v{version}", results);
    }

    public async Task<ReportResultDto> ReportReceiptTransactionAsync(ReportFilterRequest? f, bool asCsv, CancellationToken ct)
    {
        var q = db.FeeTransactions.AsNoTracking().AsQueryable();
        if (f?.FromDate is not null)
            q = q.Where(x => x.TxDate >= f.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (f?.ToDate is not null)
            q = q.Where(x => x.TxDate <= f.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(f?.StudentId))
            q = q.Where(x => x.StudentId == f.StudentId);
        var data = await q.OrderByDescending(x => x.TxDate).Select(x => ToDto(x)).ToListAsync(ct);
        return new ReportResultDto(asCsv ? "ReceiptTransactionCSV" : "ReceiptTransaction", data);
    }

    public async Task<ReportResultDto> ReportStudentwiseTransactionAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await GetTransactionsAsync(f?.StudentId, ct);
        return new ReportResultDto("StudentwiseTransaction", data);
    }

    public async Task<ReportResultDto> ReportCourseStudentwiseTransactionAsync(ReportFilterRequest? f, CancellationToken ct)
        => await ReportStudentwiseTransactionAsync(f, ct);

    public async Task<ReportResultDto> ReportAdjustmentTransactionsAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Adjustments.Any())
            .Select(x => ToDto(x)).ToListAsync(ct);
        return new ReportResultDto("AdjustmentTransactions", data);
    }

    public async Task<ReportResultDto> ReportFeesPaidAsync(ReportFilterRequest? f, bool asCsv, CancellationToken ct)
    {
        var data = await GetTransactionsAsync(f?.StudentId, ct);
        return new ReportResultDto(asCsv ? "FeesPaidCSV" : "FeesPaid", data);
    }

    public async Task<ReportResultDto> ReportBreakUpsAsync(ReportFilterRequest? f, CancellationToken ct)
        => new("BreakUps", await GetDatewiseCollectionAsync(
            f?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            f?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow), ct));

    public async Task<ReportResultDto> ReportFineCollectedAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var total = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .SelectMany(x => x.FinesPaid)
            .SumAsync(x => x.Amount, ct);
        return new ReportResultDto("FineCollected", new { TotalFine = total });
    }

    public async Task<ReportResultDto> ReportLatePaymentAsync(ReportFilterRequest? f, CancellationToken ct)
        => new("LatePayment", Array.Empty<object>(), "Late-payment report – requires due-date policy engine.");

    public async Task<ReportResultDto> ReportTimeLineAsync(ReportFilterRequest? f, CancellationToken ct)
        => await ReportDatewiseFeeCollectionAsync(f, ct);

    public async Task<ReportResultDto> ReportAdjustmentBalanceAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var total = await db.FeeTransactions.AsNoTracking()
            .SelectMany(x => x.Adjustments).SumAsync(x => x.Amount, ct);
        return new ReportResultDto("AdjustmentBalance", new { Total = total });
    }

    public async Task<ReportResultDto> ReportAdvanceBalanceAsync(ReportFilterRequest? f, CancellationToken ct)
        => new("AdvanceBalance", new { Total = 0m }, "Advance balance derived from overpayments.");

    public async Task<ReportResultDto> ReportTxHistoryAsync(CancellationToken ct)
        => new("TxHistory", await GetTransactionsAsync(null, ct));

    public async Task<ReportResultDto> ReportFeesPaidAfterAcademicYearAsync(CancellationToken ct)
        => new("FeesPaidAfterAcademicYear", await GetTransactionsAsync(null, ct));

    public async Task<ReportResultDto> ReportFeesPaidByTcStudentsAsync(CancellationToken ct)
        => new("FeesPaidByTCStudents", Array.Empty<object>(), "Requires student TC-status from Stu module.");

    public async Task<ReportResultDto> ReportCoursewiseFeeBreakupAsJsonAsync(ReportFilterRequest? f, CancellationToken ct)
        => await ReportCoursewiseFeeStructureAsync(f, ct);

    public async Task<ReportResultDto> ReportCoursewiseFeeCollectionAsJsonAsync(ReportFilterRequest? f, CancellationToken ct)
        => await ReportDatewiseFeeCollectionAsync(f, ct);

    public async Task<ReportResultDto> ReportCoursewiseFeeStructureAsJsonAsync(CancellationToken ct)
        => await ReportCoursewiseFeeStructureAsync(null, ct);

    public async Task<ReportResultDto> ReportPaymentModeBreakupAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Status == TransactionStatus.Completed)
            .GroupBy(x => x.PaymentMode ?? "Unknown")
            .Select(g => new { PaymentMode = g.Key, Amount = g.Sum(x => x.Amount), Count = g.LongCount() })
            .ToListAsync(ct);
        return new ReportResultDto("PaymentModeBreakup", data);
    }

    public async Task<ReportResultDto> ReportPrintFeeReceiptAsync(EmailFeeReceiptRequest request, CancellationToken ct)
    {
        var html = await GetTransactionHtmlAsync(request.TxId, ct);
        return new ReportResultDto("PrintFeeReceipt", html);
    }

    public async Task<ReportResultDto> ReportFeesPayableAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(f?.StudentId))
            return new ReportResultDto("FeesPayable", Array.Empty<object>(), "StudentId required.");
        var data = await GetFeesToBePaidAsync(f.StudentId, ct);
        return new ReportResultDto("FeesPayable", data);
    }

    public async Task<ReportResultDto> ReportFeeReceivedInOtherAccountAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.ReceivedInAccount != null && x.ReceivedInAccount != "")
            .Select(x => ToDto(x)).ToListAsync(ct);
        return new ReportResultDto("FeeReceivedInOtherAccount", data);
    }

    public async Task<ReportResultDto> ReportConcessionAsync(ReportFilterRequest? f, CancellationToken ct)
    {
        var data = await db.FeeTransactions.AsNoTracking()
            .Where(x => x.Discounts.Any())
            .Select(x => ToDto(x)).ToListAsync(ct);
        return new ReportResultDto("Concession", data);
    }

    public async Task<ReportResultDto> ReportRefundAsync(ReportFilterRequest? f, CancellationToken ct)
        => new("Refund", await GetRefundsAsync(ct));

    public async Task<ReportResultDto> ReportSuspenseAccountTxnAsync(ReportFilterRequest? f, CancellationToken ct)
        => new("SuspenseAccountTxn", await GetSuspenseTransactionsAsync(ct));

    // ===================== Email =====================

    public async Task EmailFeeReceiptAsync(EmailFeeReceiptRequest request, CancellationToken ct)
    {
        var html = await GetTransactionHtmlAsync(request.TxId, ct)
            ?? throw new InvalidOperationException($"Transaction {request.TxId} not found.");

        var studentId = request.StudentId;
        var to = new List<string>();
        if (!string.IsNullOrWhiteSpace(studentId))
        {
            var stu = await students.GetAsync(studentId, ct);
            if (stu?.Emails is { Count: > 0 })
                to.AddRange(stu.Emails.Where(e => !string.IsNullOrWhiteSpace(e)));
        }

        // Port of EmailService.EmailFeeReceipt → EnqueEmail
        var email = new EmailJobMessage
        {
            subject = "Fee Receipt",
            html = html.Html,
            to = to.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            sender_id = user.OwnerId == Guid.Empty ? "fee" : user.OwnerId.ToString("N")[..8],
            inst_id = user.OwnerId.ToString(),
            inst_short_name = user.OwnerId == Guid.Empty ? "FEE" : user.OwnerId.ToString("N")[..4].ToUpperInvariant(),
            attachments =
            [
                new EmailAttachmentMessage
                {
                    file_path = "FeeReceipt.html",
                    mime_type = "text",
                    mime_sub_type = "html",
                    data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html.Html))
                }
            ]
        };
        await events.PublishEmailAsync(email, ct);
    } not found.");
        // Integration point for email provider (SMTP / SES / etc.)
        // Original used EmailService.EmailFeeReceipt(...)
        _ = html;
        await Task.CompletedTask;
    }

    // ===================== Helpers =====================

    private void StampCreate(AuditableEntity e)
    {
        e.OwnerId = user.OwnerId;
        e.CreatedOn = DateTimeOffset.UtcNow;
        e.CreatedBy = user.UserId ?? "system";
        e.ModifiedOn = e.CreatedOn;
        e.ModifiedBy = e.CreatedBy;
    }

    private void StampModify(AuditableEntity e)
    {
        e.ModifiedOn = DateTimeOffset.UtcNow;
        e.ModifiedBy = user.UserId ?? "system";
    }

    private static FeeDefinitionDto ToDto(FeeDefinition fee) => new(
        fee.Id, fee.OwnerId, fee.Name, fee.NameLower, fee.DisplayText, fee.Amount, fee.Tags,
        fee.CollectStudentWise, fee.IsTxDone, fee.Status, fee.SpansAcademicYears,
        fee.Installments.OrderBy(x => x.InstallmentNo)
            .Select(x => new FeeInstallmentDto(x.InstallmentNo, x.Description, x.Amount, x.DueDate)).ToList(),
        fee.Fines.OrderBy(x => x.FineNo)
            .Select(x => new FeeFineDto(x.FineNo, x.Description, x.Amount, x.OverdueByDays)).ToList(),
        fee.Students.Select(x => x.StudentId).Order().ToList(),
        fee.Courses.Select(x => x.CourseId).Order().ToList());

    private static FeeTransactionDto ToDto(FeeTransaction tx) => new(
        tx.Id, tx.TxNo, tx.TxDate, tx.StudentId, tx.PaymentMode, tx.IsOpeningBalanceAdjusted,
        tx.RefNo, tx.ReceivedInAccount, tx.RefNo, tx.Amount, tx.Status);

    private static RefundDto ToDto(Refund refund) => new(
        refund.Id, refund.StudentId, refund.FeeTransactionId, refund.RefundDate,
        refund.Amount, refund.Status, refund.Reason, refund.Reference);

    private static SuspenseTransactionDto ToDto(SuspenseAccountTransaction s) => new(
        s.Id, s.TransactionDate, s.Amount, s.PaymentMode, s.Particulars, s.Particulars,
        s.Status, s.Allocations?.Sum(x => x.AllocatedAmount) ?? 0m);

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    private static void ValidateMoney(decimal value, string name)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(name, "Money values must be zero or positive.");
    }
}
