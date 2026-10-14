using Ct.Admission.Domain;
using Ct.Admission.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Admission.Application;

public sealed class ApplicationService(AdmissionDbContext db, IAdmissionUserContext user)
{
    public async Task<object> CreateAsync(CreateApplicationRequest req, CancellationToken ct = default)
    {
        var a = new AdmissionApplication
        {
            OwnerId = user.OwnerId,
            ApplicationNo = $"APP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100,999)}",
            Email = req.Email,
            Mobile = req.Mobile,
            Name = req.Name.Trim(),
            CourseId = req.CourseId,
            CourseName = req.CourseName,
            Category = req.Category,
            TemplateId = req.TemplateId,
            CreatedBy = user.UserId
        };
        db.Applications.Add(a);
        await db.SaveChangesAsync(ct);
        return Map(a);
    }

    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Applications.AsNoTracking()
            .Where(a => a.OwnerId == user.OwnerId)
            .OrderByDescending(a => a.CreatedOn)
            .Select(a => Map(a))
            .ToListAsync(ct);

    public async Task<object?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var a = await db.Applications.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.Email == email, ct);
        return a is null ? null : Map(a);
    }

    public async Task<bool> UpdateIdentityAsync(Guid id, Dictionary<string, string> data, CancellationToken ct = default)
        => await PatchDict(id, a => { a.Identity = data; }, ct);

    public async Task<bool> UpdateFatherAsync(Guid id, Dictionary<string, string> data, CancellationToken ct = default)
        => await PatchDict(id, a => { a.Father = data; }, ct);

    public async Task<bool> UpdateMotherAsync(Guid id, Dictionary<string, string> data, CancellationToken ct = default)
        => await PatchDict(id, a => { a.Mother = data; }, ct);

    public async Task<bool> UpdateGuardianAsync(Guid id, Dictionary<string, string> data, CancellationToken ct = default)
        => await PatchDict(id, a => { a.Guardian = data; }, ct);

    public async Task<bool> UpdateAcademicAsync(Guid id, List<Dictionary<string, string>> history, CancellationToken ct = default)
        => await PatchDict(id, a => { a.AcademicHistory = history; }, ct);

    public async Task<bool> UpdateDocumentAsync(Guid id, string key, string url, CancellationToken ct = default)
        => await PatchDict(id, a =>
        {
            a.Documents[key] = url;
            a.Documents = new Dictionary<string, string>(a.Documents);
        }, ct);

    public async Task<bool> SetStatusAsync(Guid id, ApplicationStatus status, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Status = status;
        if (status == ApplicationStatus.Submitted) a.SubmittedOn = DateTime.UtcNow;
        if (status == ApplicationStatus.Admitted)
        {
            a.AdmittedOn = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(a.CourseId))
            {
                var seat = await db.SeatMatrices.FirstOrDefaultAsync(s => s.OwnerId == user.OwnerId && s.CourseId == a.CourseId, ct);
                if (seat is not null) { seat.AdmittedCount++; seat.ModifiedOn = DateTime.UtcNow; }
            }
        }
        Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public Task<bool> AdmitAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Admitted, ct);
    public Task<bool> ShortlistAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Shortlisted, ct);
    public Task<bool> SelectAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Selected, ct);
    public Task<bool> DeclineAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Declined, ct);
    public Task<bool> RejectAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Rejected, ct);
    public Task<bool> SubmitAsync(Guid id, CancellationToken ct = default) => SetStatusAsync(id, ApplicationStatus.Submitted, ct);

    public async Task<bool> RemoveFromSelectionAsync(Guid id, CancellationToken ct = default)
        => await SetStatusAsync(id, ApplicationStatus.Submitted, ct);

    public async Task<IReadOnlyList<object>> GetAdmittedAsync(CancellationToken ct = default)
        => await db.Applications.AsNoTracking()
            .Where(a => a.OwnerId == user.OwnerId && a.Status == ApplicationStatus.Admitted)
            .Select(a => Map(a)).ToListAsync(ct);

    private async Task<bool> PatchDict(Guid id, Action<AdmissionApplication> patch, CancellationToken ct)
    {
        var a = await Load(id, ct); if (a is null) return false;
        patch(a); Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    private async Task<AdmissionApplication?> Load(Guid id, CancellationToken ct)
        => await db.Applications.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private void Stamp(AdmissionApplication a) { a.ModifiedBy = user.UserId; a.ModifiedOn = DateTime.UtcNow; }

    private static object Map(AdmissionApplication a) => new
    {
        a.Id, a.ApplicationNo, a.Email, a.Mobile, a.Name, a.CourseId, a.CourseName,
        Status = a.Status.ToString(), a.Category, a.Identity, a.Father, a.Mother, a.Guardian,
        a.AcademicHistory, a.Documents, a.TemplateId, a.CreatedOn, a.SubmittedOn, a.AdmittedOn
    };
}

public sealed class CreateApplicationRequest
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? Category { get; set; }
    public Guid? TemplateId { get; set; }
}

public sealed class AdmissionService(AdmissionDbContext db, IAdmissionUserContext user)
{
    public async Task<object> CreateSeatMatrixAsync(SeatMatrixRequest req, CancellationToken ct = default)
    {
        var existing = await db.SeatMatrices.FirstOrDefaultAsync(s => s.OwnerId == user.OwnerId && s.CourseId == req.CourseId, ct);
        if (existing is not null)
        {
            existing.TotalSeats = req.TotalSeats;
            existing.ReservedSeats = req.ReservedSeats;
            existing.CourseName = req.CourseName;
            existing.CategoryQuota = req.CategoryQuota ?? existing.CategoryQuota;
            existing.ModifiedOn = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Map(existing);
        }
        var s = new SeatMatrix
        {
            OwnerId = user.OwnerId,
            CourseId = req.CourseId,
            CourseName = req.CourseName,
            TotalSeats = req.TotalSeats,
            ReservedSeats = req.ReservedSeats,
            CategoryQuota = req.CategoryQuota ?? new()
        };
        db.SeatMatrices.Add(s);
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<object?> UpdateSeatMatrixAsync(Guid id, SeatMatrixRequest req, CancellationToken ct = default)
    {
        var s = await db.SeatMatrices.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (s is null) return null;
        s.TotalSeats = req.TotalSeats;
        s.ReservedSeats = req.ReservedSeats;
        if (req.CourseName is not null) s.CourseName = req.CourseName;
        if (req.CategoryQuota is not null) s.CategoryQuota = req.CategoryQuota;
        s.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(s);
    }

    public async Task<IReadOnlyList<object>> GetSeatMatricesAsync(CancellationToken ct = default)
        => await db.SeatMatrices.AsNoTracking().Where(s => s.OwnerId == user.OwnerId).Select(s => Map(s)).ToListAsync(ct);

    public async Task<object?> GetSeatsAvailableAsync(string courseId, CancellationToken ct = default)
    {
        var s = await db.SeatMatrices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.CourseId == courseId, ct);
        if (s is null) return null;
        return new
        {
            s.CourseId, s.CourseName, s.TotalSeats, s.ReservedSeats, s.AdmittedCount,
            available = Math.Max(0, s.TotalSeats - s.ReservedSeats - s.AdmittedCount)
        };
    }

    public async Task<object> ApplicationsCoursewiseAsync(CancellationToken ct = default)
    {
        var list = await db.Applications.AsNoTracking().Where(a => a.OwnerId == user.OwnerId).ToListAsync(ct);
        return list.GroupBy(a => a.CourseId ?? "unknown")
            .Select(g => new
            {
                courseId = g.Key,
                total = g.Count(),
                byStatus = g.GroupBy(x => x.Status.ToString()).Select(s => new { status = s.Key, count = s.Count() })
            });
    }

    public async Task<object> GetAdmissionsAsync(CancellationToken ct = default)
    {
        var admitted = await db.Applications.AsNoTracking()
            .Where(a => a.OwnerId == user.OwnerId && a.Status == ApplicationStatus.Admitted)
            .ToListAsync(ct);
        return new { count = admitted.Count, applications = admitted.Select(a => new { a.Id, a.ApplicationNo, a.Name, a.CourseId, a.AdmittedOn }) };
    }

    private static object Map(SeatMatrix s) => new
    {
        s.Id, s.CourseId, s.CourseName, s.TotalSeats, s.ReservedSeats, s.AdmittedCount, s.CategoryQuota,
        available = Math.Max(0, s.TotalSeats - s.ReservedSeats - s.AdmittedCount)
    };
}

public sealed class SeatMatrixRequest
{
    public required string CourseId { get; set; }
    public string? CourseName { get; set; }
    public int TotalSeats { get; set; }
    public int ReservedSeats { get; set; }
    public Dictionary<string, int>? CategoryQuota { get; set; }
}

public sealed class TemplateService(AdmissionDbContext db, IAdmissionUserContext user)
{
    public async Task<object> CreateAsync(CreateTemplateRequest req, CancellationToken ct = default)
    {
        var t = new ApplicationFormTemplate
        {
            OwnerId = user.OwnerId,
            Title = req.Title.Trim(),
            Description = req.Description,
            Fields = req.Fields ?? [],
            ShortlistCriteria = req.ShortlistCriteria ?? [],
            CreatedBy = user.UserId
        };
        db.Templates.Add(t);
        await db.SaveChangesAsync(ct);
        return Map(t);
    }

    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Templates.AsNoTracking().Where(t => t.OwnerId == user.OwnerId).Select(t => Map(t)).ToListAsync(ct);

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await db.Templates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return t is null ? null : Map(t);
    }

    public async Task<object?> UpdateAsync(Guid id, CreateTemplateRequest req, CancellationToken ct = default)
    {
        var t = await Load(id, ct); if (t is null) return null;
        t.Title = req.Title.Trim();
        if (req.Description is not null) t.Description = req.Description;
        if (req.Fields is not null) t.Fields = req.Fields;
        if (req.ShortlistCriteria is not null) t.ShortlistCriteria = req.ShortlistCriteria;
        t.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(t);
    }

    public async Task<bool> SetStatusAsync(Guid id, TemplateStatus status, CancellationToken ct = default)
    {
        var t = await Load(id, ct); if (t is null) return false;
        t.Status = status; t.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddShortlistAsync(Guid id, string criterion, CancellationToken ct = default)
    {
        var t = await Load(id, ct); if (t is null) return false;
        t.ShortlistCriteria.Add(criterion);
        t.ShortlistCriteria = t.ShortlistCriteria.ToList();
        t.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateShortlistAsync(Guid id, List<string> criteria, CancellationToken ct = default)
    {
        var t = await Load(id, ct); if (t is null) return false;
        t.ShortlistCriteria = criteria; t.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object?> GetShortlistAsync(Guid id, CancellationToken ct = default)
    {
        var t = await db.Templates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return t is null ? null : new { t.Id, t.Title, shortlist = t.ShortlistCriteria };
    }

    private async Task<ApplicationFormTemplate?> Load(Guid id, CancellationToken ct)
        => await db.Templates.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private static object Map(ApplicationFormTemplate t) => new
    {
        t.Id, t.Title, t.Description, Status = t.Status.ToString(), t.Fields, t.ShortlistCriteria, t.CreatedOn
    };
}

public sealed class CreateTemplateRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Fields { get; set; }
    public List<string>? ShortlistCriteria { get; set; }
}

public sealed class OtpService(AdmissionDbContext db)
{
    public async Task<object> RequestOtpAsync(string mobileOrEmail, CancellationToken ct = default)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var rec = new OtpRecord
        {
            MobileOrEmail = mobileOrEmail.Trim(),
            Code = code,
            ExpiresOn = DateTime.UtcNow.AddMinutes(10)
        };
        db.Otps.Add(rec);
        await db.SaveChangesAsync(ct);
        // In production, send via Connect SMS/email; return code only in dev
        return new { sent = true, expiresOn = rec.ExpiresOn, debugCode = code };
    }
}

public sealed class AdmissionCmd(ApplicationService apps, AdmissionService adm, TemplateService templates)
{
    public Task<object> CreateApplication(CreateApplicationRequest r, CancellationToken ct = default) => apps.CreateAsync(r, ct);
    public Task<bool> Admit(Guid id, CancellationToken ct = default) => apps.AdmitAsync(id, ct);
    public Task<bool> Shortlist(Guid id, CancellationToken ct = default) => apps.ShortlistAsync(id, ct);
    public Task<bool> Select(Guid id, CancellationToken ct = default) => apps.SelectAsync(id, ct);
    public Task<bool> Decline(Guid id, CancellationToken ct = default) => apps.DeclineAsync(id, ct);
    public Task<bool> Reject(Guid id, CancellationToken ct = default) => apps.RejectAsync(id, ct);
    public Task<bool> Submit(Guid id, CancellationToken ct = default) => apps.SubmitAsync(id, ct);
    public Task<object> SeatMatrix(SeatMatrixRequest r, CancellationToken ct = default) => adm.CreateSeatMatrixAsync(r, ct);
}

public sealed class AdmissionQry(ApplicationService apps, AdmissionService adm)
{
    public Task<IReadOnlyList<object>> GetApplications(CancellationToken ct = default) => apps.GetAllAsync(ct);
    public Task<object?> GetApplicationByEmail(string email, CancellationToken ct = default) => apps.GetByEmailAsync(email, ct);
    public Task<IReadOnlyList<object>> GetAdmitted(CancellationToken ct = default) => apps.GetAdmittedAsync(ct);
    public Task<IReadOnlyList<object>> GetSeatMatrices(CancellationToken ct = default) => adm.GetSeatMatricesAsync(ct);
    public Task<object?> GetSeatsAvailable(string courseId, CancellationToken ct = default) => adm.GetSeatsAvailableAsync(courseId, ct);
    public Task<object> ApplicationsCoursewise(CancellationToken ct = default) => adm.ApplicationsCoursewiseAsync(ct);
    public Task<object> GetAdmissions(CancellationToken ct = default) => adm.GetAdmissionsAsync(ct);
}
