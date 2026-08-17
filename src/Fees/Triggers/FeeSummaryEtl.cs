namespace Fees.Triggers;

/// <summary>
/// Fee-to-student aggregation is now maintained by PostgreSQL triggers.
/// The trigger updates campus.student_fee_summary_cache whenever a receivable
/// or opening balance changes; campus.v_student_fee_summary exposes the read model.
/// </summary>
public static class FeeSummaryTrigger
{
    public const string TriggerName = "trg_refresh_student_fee_summary";
}
