using Ct.Modules.Fees.Application;
using Ct.Modules.Fees.Domain;
using Ct.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ct.Modules.Fees.Infrastructure;

public sealed class FeeQueryService(AppDbContext db) : IFeeQueries
{
    public async Task<IReadOnlyList<StudentFeeSummary>> GetFeeSummaryAsync(Guid? studentId, CancellationToken ct)
    {
        var query = db.StudentFeeSummaries.AsNoTracking();
        if (studentId.HasValue)
            query = query.Where(x => x.StudentPk == studentId.Value);

        return await query.ToListAsync(ct);
    }
}
