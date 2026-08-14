using Fees.Repo.Resource;
using Marten;
using Student.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fees.Triggers
{
    /// <summary>
    /// Postgres / Marten replacement for the old RavenDB ETL "trigger".
    ///
    /// Pattern (unchanged conceptually):
    ///   Fees owns FeesReceivable writes
    ///       -- this publisher (the "trigger") -->
    ///   Student module's StudentFeeSummary collection (the "view", read-only in Student)
    ///
    /// Implementation options on Postgres:
    ///   A) Application-level: call PublishAsync after FeesCmd saves (this class).
    ///   B) SQL TRIGGER + LISTEN/NOTIFY + background worker.
    ///   C) Marten async daemon projection across databases (advanced).
    ///
    /// This class implements (A) — simplest and keeps the C# view type intact.
    /// </summary>
    public static class FeeSummaryEtl
    {
        /// <summary>
        /// Upsert a StudentFeeSummary document into the Student Marten store
        /// after a fees change. Call from FeesCmd after SaveChanges.
        /// </summary>
        public static async Task PublishAsync(
            IDocumentStore studentStore,
            string studentId,
            string instId,
            decimal totalReceivable,
            decimal totalPaid,
            decimal openingBalance,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(studentId);
            ArgumentNullException.ThrowIfNull(studentStore);

            await using var session = studentStore.LightweightSession();
            var summary = new StudentFeeSummary
            {
                StudentId = studentId,
                InstId = instId,
                TotalReceivable = totalReceivable,
                TotalPaid = totalPaid,
                OpeningBalance = openingBalance,
                LastUpdatedOn = DateTime.UtcNow
            };
            session.Store(summary);
            await session.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Optional: aggregate from FeesReceivable documents for one student, then publish.
        /// </summary>
        public static async Task RebuildAndPublishAsync(
            IDocumentStore feesStore,
            IDocumentStore studentStore,
            string studentId,
            string instId,
            CancellationToken ct = default)
        {
            await using var feesSession = feesStore.QuerySession();
            var receivables = await feesSession.Query<FeesReceivable>()
                .Where(x => x.StudentId == studentId)
                .ToListAsync(ct);

            decimal totalReceivable = 0;
            decimal totalPaid = 0;
            decimal opening = 0;
            foreach (var r in receivables)
            {
                // Adjust property names to match your FeesReceivable shape
                totalReceivable += r.Amount;
                // totalPaid += r.Paid; etc. when fields exist
            }

            await PublishAsync(studentStore, studentId, instId, totalReceivable, totalPaid, opening, ct);
        }

        /// <summary>
        /// Kept for deploy-time parity with the old EnsureConfigured(Raven ETL) API.
        /// With Marten, schema is created by DocumentStore auto-create / migrations —
        /// no Raven ETL task registration is required.
        /// </summary>
        public static void EnsureConfigured(IDocumentStore feesStore, IDocumentStore studentStore)
        {
            // No-op: Marten stores are configured in FeesModule / StudentModule.
            // StudentFeeSummary identity is registered on the Student store.
            _ = feesStore;
            _ = studentStore;
        }
    }
}
