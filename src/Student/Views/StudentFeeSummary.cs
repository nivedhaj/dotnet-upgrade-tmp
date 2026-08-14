using System;

namespace Student.Views
{
    /// <summary>
    /// Read-only projection of a student's fee position, materialized in the STUDENT
    /// module's own Postgres schema (via Marten). This is the "view" half of the
    /// trigger/view pattern:
    ///
    ///   Fees module (owns the data, writes changes)
    ///       -- FeeSummaryEtl.PublishAsync (the "trigger") -->
    ///   Student module's StudentFeeSummary (the "view", read-only here)
    ///
    /// Student never calls into Fees to read this — it queries this document locally.
    /// See Student/Triggers/README.md and Fees/Triggers/FeeSummaryEtl.cs.
    ///
    /// Written ONLY by the ETL/publisher. Nothing in Student's Cmd layer should mutate it.
    /// </summary>
    public class StudentFeeSummary
    {
        /// <summary>Same value as the Student document's Id.</summary>
        public string StudentId { get; set; } = string.Empty;
        public string InstId { get; set; } = string.Empty;
        public decimal TotalReceivable { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal OutstandingBalance => TotalReceivable - TotalPaid;
        public DateTime LastUpdatedOn { get; set; }
    }
}
