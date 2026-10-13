using Microsoft.Extensions.Configuration;

namespace Ct.Fee.Application.Policy;

public class DefaultFeePolicyEngine : IFeePolicyEngine
{
    public virtual int FeeOrderIndex(string feeName) =>
        feeName.StartsWith("Tuition", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

    public virtual IReadOnlyList<string> CourseOrder() =>
        ["I", "II", "III", "IV", "V", "VI", "VII"];

    public virtual IReadOnlyList<string> ExcludeFeesForRte(string? academicYear) => [];

    public virtual bool SplitTuitionFees() => false;

    public virtual string? GetFeesToBeSplit(int? academicYearFrom) => null;

    public virtual IReadOnlyList<string> GetDatewiseReportCols(
        IReadOnlyList<string> cols, string? feeName, int? academicYearFrom) => cols;

    public virtual bool IncludeRteStudents() => true;

    public virtual IReadOnlyList<string> GetFeeCollectionAccounts() =>
        ["Cash", "Bank - Primary", "Online / Gateway", "DefaultAccount"];

    public virtual string? GetClassGroup(string? courseName) => courseName;

    public virtual FeeColumnMapping? ClassifyFeeName(string? feeName) => null;
}

/// <summary>Port of original BTLCOLPolicyEngine (collection accounts + RTE handling).</summary>
public sealed class BtlColFeePolicyEngine : DefaultFeePolicyEngine
{
    public override bool IncludeRteStudents() => false;

    public override IReadOnlyList<string> ExcludeFeesForRte(string? academicYear) =>
        ["Tuition", "Transport"];

    public override IReadOnlyList<string> GetFeeCollectionAccounts() =>
        ["Cash", "Bank - BTL", "UPI", "Cheque"];
}

/// <summary>Port of original VVSGCSPolicyEngine (fee name classification for nursery/primary).</summary>
public sealed class VvsGcsFeePolicyEngine : DefaultFeePolicyEngine
{
    private static readonly Dictionary<string, FeeColumnMapping> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CEP - LKG & UKG"] = new("NURSERY", "CEP"),
        ["CEP Pre KG"] = new("PRE KG", "CEP"),
        ["CEP - Std 1 - 4"] = new("I - X", "CEP"),
        ["CEP - Std 5 - 10"] = new("I - X", "CEP"),
        ["Miscellaneous LKG & UKG"] = new("NURSERY", "MISC"),
        ["Miscellaneous Pre KG"] = new("PRE KG", "MISC"),
        ["Miscellaneous - Std I - X"] = new("I - X", "MISC"),
        ["Next Education fee"] = new("I - X", "NE FEE"),
        ["SDF"] = new("I - X", "SDF"),
        ["Tuition fees for LKG & UKG"] = new("NURSERY", "TF"),
        ["Tuition fee Pre KG"] = new("PRE KG", "TF"),
        ["Tuition fee Std I"] = new("I - X", "TF"),
        ["Tuition fee Std II - IV"] = new("I - X", "TF"),
        ["Tuition fees Std V - X"] = new("I - X", "TF"),
    };

    public override bool SplitTuitionFees() => true;

    public override FeeColumnMapping? ClassifyFeeName(string? feeName)
    {
        if (string.IsNullOrWhiteSpace(feeName)) return null;
        return Map.TryGetValue(feeName.Trim(), out var m) ? m : null;
    }

    public override IReadOnlyList<string> GetFeeCollectionAccounts() =>
        ["Cash", "Bank - VVSGCS", "Online"];
}

public sealed class VvsPhsFeePolicyEngine : DefaultFeePolicyEngine
{
    public override IReadOnlyList<string> GetFeeCollectionAccounts() =>
        ["Cash", "Bank - VVSPHS", "Online"];
}

public sealed class VvsBmsFeePolicyEngine : DefaultFeePolicyEngine
{
    public override IReadOnlyList<string> GetFeeCollectionAccounts() =>
        ["Cash", "Bank - VVSBMS", "Online"];
}

public sealed class FeePolicyEngineProvider(IConfiguration config) : IFeePolicyEngineProvider
{
    private readonly string _defaultPolicy =
        config["Fee:PolicyEngine"] ?? config["FeeModule:PolicyEngine"] ?? "Default";

    public IFeePolicyEngine For(string? policyId)
    {
        var key = (policyId ?? _defaultPolicy).Trim();
        return key.ToUpperInvariant() switch
        {
            "BTLCOL" or "BTL" => new BtlColFeePolicyEngine(),
            "VVSGCS" or "GCS" => new VvsGcsFeePolicyEngine(),
            "VVSPHS" or "PHS" => new VvsPhsFeePolicyEngine(),
            "VVSBMS" or "BMS" => new VvsBmsFeePolicyEngine(),
            _ => new DefaultFeePolicyEngine()
        };
    }
}
