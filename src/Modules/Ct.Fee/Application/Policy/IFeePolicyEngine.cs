namespace Ct.Fee.Application.Policy;

public interface IFeePolicyEngine
{
    int FeeOrderIndex(string feeName);
    IReadOnlyList<string> CourseOrder();
    IReadOnlyList<string> ExcludeFeesForRte(string? academicYear);
    bool SplitTuitionFees();
    string? GetFeesToBeSplit(int? academicYearFrom);
    IReadOnlyList<string> GetDatewiseReportCols(IReadOnlyList<string> cols, string? feeName, int? academicYearFrom);
    bool IncludeRteStudents();
    IReadOnlyList<string> GetFeeCollectionAccounts();
    string? GetClassGroup(string? courseName);
    FeeColumnMapping? ClassifyFeeName(string? feeName);
}

public sealed record FeeColumnMapping(string GroupName, string FeeCode);

public interface IFeePolicyEngineProvider
{
    IFeePolicyEngine For(string? policyId);
}

public enum FeePolicyEngineKind
{
    Default,
    BtlCol,
    VvsGcs,
    VvsPhs,
    VvsBms
}
