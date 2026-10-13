using Ct.Staff.Domain;
using Ct.Staff.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Staff.Application;

public sealed class StaffService(StaffDbContext db, IStaffUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .OrderBy(s => s.Name)
            .Select(s => Map(s))
            .ToListAsync(ct);

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var s = await db.Staff.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return s is null ? null : Map(s);
    }

    public async Task<object?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var s = await db.Staff.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.PrimaryEmail == email, ct);
        return s is null ? null : Map(s);
    }

    public async Task<object> CreateAsync(CreateStaffRequest req, CancellationToken ct = default)
    {
        var s = new StaffMember
        {
            OwnerId = user.OwnerId,
            Name = req.Name.Trim(),
            StaffCode = req.StaffCode,
            Gender = req.Gender,
            PrimaryEmail = req.PrimaryEmail,
            PrimaryMobile = req.PrimaryMobile,
            Designation = req.Designation,
            StaffType = ParseType(req.StaffType),
            InstituteIds = req.InstituteIds ?? [],
            Subjects = req.Subjects ?? [],
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Staff.Add(s);
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<object?> UpdateAsync(Guid id, CreateStaffRequest req, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return null;
        s.Name = req.Name.Trim();
        if (req.StaffCode is not null) s.StaffCode = req.StaffCode;
        if (req.Gender is not null) s.Gender = req.Gender;
        if (req.PrimaryEmail is not null) s.PrimaryEmail = req.PrimaryEmail;
        if (req.PrimaryMobile is not null) s.PrimaryMobile = req.PrimaryMobile;
        if (req.Designation is not null) s.Designation = req.Designation;
        if (req.StaffType is not null) s.StaffType = ParseType(req.StaffType);
        if (req.InstituteIds is not null) s.InstituteIds = req.InstituteIds;
        if (req.Subjects is not null) s.Subjects = req.Subjects;
        Stamp(s); await db.SaveChangesAsync(ct); return Map(s);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Status = StaffStatus.Active; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Status = StaffStatus.Disabled; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AssignInstitutesAsync(Guid id, List<string> instIds, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.InstituteIds = instIds.Distinct().ToList(); Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AssignSubjectsAsync(Guid id, List<string> subjects, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Subjects = subjects.Distinct().ToList(); Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetAliasAsync(Guid id, string alias, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Alias = alias; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetClassTeacherAsync(Guid id, string? courseId, string? section, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.ClassTeacherCourseId = courseId; s.ClassTeacherSection = section;
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetPrimaryEmailAsync(Guid id, string email, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.PrimaryEmail = email; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetPrimaryMobileAsync(Guid id, string mobile, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.PrimaryMobile = mobile; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateGenderAsync(Guid id, string gender, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Gender = gender; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateDesignationAsync(Guid id, string designation, string? instId, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Designation = designation;
        s.Designations.Add(new DesignationRecord
        {
            InstId = instId ?? user.InstId ?? "",
            Title = designation,
            From = DateTime.UtcNow,
            StaffType = s.StaffType
        });
        s.Designations = s.Designations.ToList();
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> EmployAsync(Guid id, string instId, string? designation, string? refId, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.EmploymentHistory.Add(new EmploymentRecord
        {
            InstId = instId,
            Designation = designation,
            From = DateTime.UtcNow,
            RefId = refId,
            Status = EmploymentStatus.Active
        });
        if (!s.InstituteIds.Contains(instId)) s.InstituteIds.Add(instId);
        s.EmploymentHistory = s.EmploymentHistory.ToList();
        s.InstituteIds = s.InstituteIds.ToList();
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AssignStudentIdAsync(Guid id, string refId, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.RefId = refId; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object?> GetTimetableAsync(Guid id, CancellationToken ct = default)
    {
        var s = await db.Staff.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return s is null ? null : s.Timetable;
    }

    public async Task<bool> SetTimetableAsync(Guid id, List<TtSlot> slots, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Timetable = slots; Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<IReadOnlyList<object>> ForInstituteAsync(string instId, CancellationToken ct = default)
    {
        var all = await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .ToListAsync(ct);
        return all.Where(s => s.InstituteIds.Contains(instId)).Select(Map).ToList();
    }

    public async Task<object> FacetAsync(CancellationToken ct = default)
    {
        var all = await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .ToListAsync(ct);
        return new
        {
            total = all.Count,
            byType = all.GroupBy(s => s.StaffType.ToString()).Select(g => new { type = g.Key, count = g.Count() }),
            byDesignation = all.GroupBy(s => s.Designation ?? "unknown").Select(g => new { designation = g.Key, count = g.Count() }),
            institutes = all.SelectMany(s => s.InstituteIds).Distinct().ToList()
        };
    }

    public async Task<bool> SalaryAddAsync(Guid id, string instId, List<SalaryItem> earnings, List<SalaryItem> deductions, DateTime? date, bool payslip, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        var nextId = s.Salaries.Count == 0 ? 1 : s.Salaries.Max(x => x.Id) + 1;
        s.Salaries.Add(new SalaryRecord
        {
            Id = nextId,
            InstId = instId,
            Earnings = earnings,
            Deductions = deductions,
            EffectiveDate = date ?? DateTime.UtcNow,
            IsPayslip = payslip
        });
        s.Salaries = s.Salaries.ToList();
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SalaryUpdateAsync(Guid id, int salaryId, List<SalaryItem>? earnings, List<SalaryItem>? deductions, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        var sal = s.Salaries.FirstOrDefault(x => x.Id == salaryId);
        if (sal is null) return false;
        if (earnings is not null) sal.Earnings = earnings;
        if (deductions is not null) sal.Deductions = deductions;
        s.Salaries = s.Salaries.ToList();
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SalaryDeleteAsync(Guid id, int salaryId, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        s.Salaries = s.Salaries.Where(x => x.Id != salaryId).ToList();
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object?> GetPayslipAsync(Guid id, int? salaryId, CancellationToken ct = default)
    {
        var s = await db.Staff.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (s is null) return null;
        var sal = salaryId is null
            ? s.Salaries.OrderByDescending(x => x.EffectiveDate).FirstOrDefault()
            : s.Salaries.FirstOrDefault(x => x.Id == salaryId);
        if (sal is null) return null;
        var gross = sal.Earnings.Sum(e => e.Amount);
        var ded = sal.Deductions.Sum(d => d.Amount);
        return new
        {
            staffId = s.Id,
            s.Name,
            s.Designation,
            sal.InstId,
            sal.EffectiveDate,
            sal.Earnings,
            sal.Deductions,
            gross,
            deductions = ded,
            net = gross - ded,
            html = $"<h2>Payslip — {System.Net.WebUtility.HtmlEncode(s.Name)}</h2><p>Net: {gross - ded:N2}</p>"
        };
    }


    public async Task<object> GetAttendanceAsync(Guid? staffId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        // Attendance events can be extended; for now derive from Meta keys attendance:*
        var q = db.Staff.AsNoTracking().Where(s => s.OwnerId == user.OwnerId);
        if (staffId is not null) q = q.Where(s => s.Id == staffId);
        var list = await q.ToListAsync(ct);
        var rows = new List<object>();
        foreach (var s in list)
        {
            foreach (var kv in s.Meta.Where(m => m.Key.StartsWith("attendance:", StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(new { staffId = s.Id, s.Name, key = kv.Key, value = kv.Value });
            }
        }
        return new { from, to, staffId, records = rows, note = "Post attendance via applyAttendance or meta keys." };
    }

    public async Task<bool> RecordAttendanceAsync(Guid id, DateTime date, string status, int sessionNumber, CancellationToken ct = default)
    {
        var s = await Load(id, ct); if (s is null) return false;
        var key = $"attendance:{date:yyyy-MM-dd}:{sessionNumber}";
        s.Meta[key] = status;
        s.Meta = new Dictionary<string, string>(s.Meta);
        Stamp(s); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object> GetStaffCountInstitutewiseAsync(CancellationToken ct = default)
    {
        var all = await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .ToListAsync(ct);
        var byInst = all.SelectMany(s => s.InstituteIds.DefaultIfEmpty("unassigned").Select(i => new { Inst = i, s.Id }))
            .GroupBy(x => x.Inst)
            .Select(g => new { instId = g.Key, count = g.Select(x => x.Id).Distinct().Count() })
            .ToList();
        return new { total = all.Count, byInstitute = byInst };
    }

    public async Task<object> GetStaffCountInstituteGenderwiseAsync(CancellationToken ct = default)
    {
        var all = await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .ToListAsync(ct);
        var rows = all.SelectMany(s => s.InstituteIds.DefaultIfEmpty("unassigned").Select(i => new
        {
            Inst = i,
            Gender = string.IsNullOrWhiteSpace(s.Gender) ? "unknown" : s.Gender
        }))
        .GroupBy(x => new { x.Inst, x.Gender })
        .Select(g => new { instId = g.Key.Inst, gender = g.Key.Gender, count = g.Count() })
        .ToList();
        return new { total = all.Count, rows };
    }

    public async Task<object> GetSmsRecipientsAsync(List<string>? teaching, List<string>? nonTeaching, List<string>? management, CancellationToken ct = default)
    {
        var all = await db.Staff.AsNoTracking()
            .Where(s => s.OwnerId == user.OwnerId && s.Status != StaffStatus.Disabled)
            .ToListAsync(ct);
        bool Match(StaffMember s, List<string>? ids, StaffType type)
        {
            if (ids is null || ids.Count == 0) return false;
            if (ids.Contains("*") || ids.Contains("all")) return s.StaffType == type;
            return s.StaffType == type && (ids.Contains(s.Id.ToString()) || (!string.IsNullOrEmpty(s.StaffCode) && ids.Contains(s.StaffCode)));
        }
        var selected = all.Where(s =>
            Match(s, teaching, StaffType.Teaching) ||
            Match(s, nonTeaching, StaffType.NonTeaching) ||
            Match(s, management, StaffType.Management)).ToList();
        return selected.Select(s => new { name = s.Name, mobile = s.PrimaryMobile, email = s.PrimaryEmail, id = s.Id, type = s.StaffType.ToString() }).ToList();
    }

    public async Task<object> GetEmailRecipientsAsync(List<string>? teaching, List<string>? nonTeaching, List<string>? management, CancellationToken ct = default)
        => await GetSmsRecipientsAsync(teaching, nonTeaching, management, ct);

    public object SmsRecipientsStub() => new { message = "Enqueue SMS via Ct.Connect recipients API." };
    public object EmailRecipientsStub() => new { message = "Enqueue email via Ct.Connect recipients API." };

    private async Task<StaffMember?> Load(Guid id, CancellationToken ct)
        => await db.Staff.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private void Stamp(StaffMember s) { s.ModifiedBy = user.UserId; s.ModifiedOn = DateTime.UtcNow; }

    private static StaffType ParseType(string? t)
        => Enum.TryParse<StaffType>(t, true, out var v) ? v : StaffType.Teaching;

    private static object Map(StaffMember s) => new
    {
        s.Id, s.StaffCode, s.Name, s.Alias, s.Gender, s.PrimaryEmail, s.PrimaryMobile,
        s.Designation, StaffType = s.StaffType.ToString(), Status = s.Status.ToString(),
        s.RefId, s.InstituteIds, s.Subjects, s.ClassTeacherCourseId, s.ClassTeacherSection,
        s.CreatedOn, EmploymentCount = s.EmploymentHistory.Count, SalaryCount = s.Salaries.Count
    };
}

public sealed class CreateStaffRequest
{
    public required string Name { get; set; }
    public string? StaffCode { get; set; }
    public string? Gender { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryMobile { get; set; }
    public string? Designation { get; set; }
    public string? StaffType { get; set; }
    public List<string>? InstituteIds { get; set; }
    public List<string>? Subjects { get; set; }
}

public sealed class SalaryService(StaffDbContext db, IStaffUserContext user)
{
    public async Task<object> CreateStructureAsync(CreateSalaryStructureRequest req, CancellationToken ct = default)
    {
        // mark previous current as not current
        var prev = await db.SalaryStructures.Where(x => x.OwnerId == user.OwnerId && x.IsCurrent).ToListAsync(ct);
        foreach (var p in prev) p.IsCurrent = false;

        var ss = new SalaryStructure
        {
            OwnerId = user.OwnerId,
            Earnings = req.Earnings ?? new(),
            Deductions = req.Deductions ?? new(),
            EffectiveFrom = req.EffectiveFrom ?? DateTime.UtcNow,
            IsCurrent = true,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.SalaryStructures.Add(ss);
        await db.SaveChangesAsync(ct);
        return Map(ss);
    }

    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.SalaryStructures.AsNoTracking()
            .Where(x => x.OwnerId == user.OwnerId)
            .OrderByDescending(x => x.EffectiveFrom)
            .Select(x => Map(x))
            .ToListAsync(ct);

    public async Task<object?> GetCurrentAsync(CancellationToken ct = default)
    {
        var ss = await db.SalaryStructures.AsNoTracking()
            .Where(x => x.OwnerId == user.OwnerId && x.IsCurrent)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
        return ss is null ? null : Map(ss);
    }

    private static object Map(SalaryStructure ss) => new
    {
        ss.Id, ss.Earnings, ss.Deductions, ss.EffectiveFrom, ss.IsCurrent, ss.CreatedOn
    };
}

public sealed class CreateSalaryStructureRequest
{
    public Dictionary<string, decimal>? Earnings { get; set; }
    public Dictionary<string, decimal>? Deductions { get; set; }
    public DateTime? EffectiveFrom { get; set; }
}

public sealed class StaffReportService(StaffService staff)
{
    public Task<IReadOnlyList<object>> StaffListAsync(CancellationToken ct = default)
        => staff.GetAllAsync(ct);

    public async Task<object> TodayTimetableAsync(Guid? staffId, CancellationToken ct = default)
    {
        if (staffId is null) return new { message = "Provide staffId" };
        var tt = await staff.GetTimetableAsync(staffId.Value, ct);
        var today = DateTime.UtcNow.DayOfWeek.ToString();
        if (tt is IEnumerable<TtSlot> slots)
            return new { day = today, slots = slots.Where(s => s.DayOfWeek.Equals(today, StringComparison.OrdinalIgnoreCase)) };
        return new { day = today, slots = tt };
    }

    public Task<object?> PayslipAsync(Guid staffId, int? salaryId, CancellationToken ct = default)
        => staff.GetPayslipAsync(staffId, salaryId, ct);
}
