using System;

namespace Student.Views
{
    /// <summary>
    /// Read-only projection of a student's fee position, materialized inside the STUDENT
    /// module's own database. This is the "view" half of the trigger/view pattern:
    ///
    ///   Fees module (owns the data, writes changes)
    ///       --ETL / subscription (the "trigger")-->
    ///   Student module's "StudentFeeSummary" collection (the "view", read-only here)
    ///
    /// Student never calls into Fees to read this — it queries this collection locally,
    /// exactly like any other document in its own database. See Student/Triggers/README.md
    /// and Fees/Triggers/FeeSummaryEtl.cs for the publishing side.
    ///
    /// This document is written ONLY by the ETL process below. Nothing in the Student
    /// module's Cmd layer should ever construct or mutate one directly.
    /// </summary>
    public class StudentFeeSummary
    {
        /// <summary>
        /// Same value as the Student document's Id — this is how the two collections
        /// line up without a live foreign-key relationship.
        /// </summary>
        public string StudentId { get; set; }
        public string InstId { get; set; }
        public decimal TotalReceivable { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal OutstandingBalance { get { return TotalReceivable - TotalPaid; } }
        public DateTime LastUpdatedOn { get; set; }
    }
}
