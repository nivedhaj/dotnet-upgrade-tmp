using Ct.Modules.Fees.Domain;

namespace Ct.Modules.Fees.Application;

public interface IFeeQueries
{
    Task<IReadOnlyList<StudentFeeSummary>> GetFeeSummaryAsync(Guid? studentId, CancellationToken ct);
}
