using ct._Core.Repo.Resource;
using System;

namespace Fees.Repo.Resource
{
    /// <summary>
    /// Fee-transaction status. Moved from the Student module — this only ever
    /// described fee transactions, never student state.
    /// </summary>
    public enum TxStatusEnum
    {
        Unknown = -1,
        Active = 1,
        Disabled = 99
    }

    /// <summary>
    /// A single receivable line item, owned by the Fees module.
    ///
    /// Previously this was a sub-document embedded inside Student.FeesReceivable,
    /// living in the same shared "btl" database as everything else. It is now a
    /// first-class Marten/Postgres document in the Fees module's own database, keyed by
    /// StudentId so the Fees module can still query "all receivables for a student"
    /// without ever touching the Student database.
    /// </summary>
    public class FeesReceivable : Entity
    {
        /// <summary>
        /// Id of the student this receivable belongs to. This is the ONLY reference
        /// back to the Student module — an opaque id, never a live object graph.
        /// </summary>
        public string StudentId { get; set; }
        public string InstId { get; set; }
        public string FeeId { get; set; }
        public string FeeName { get; set; }
        public string TxId { get; set; }
        public TxStatusEnum TxStatus { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// A student's opening balance, owned by the Fees module. Also promoted out of
    /// the embedded Student sub-document into its own first-class document.
    /// </summary>
    public class OpeningBalance : Entity
    {
        public string StudentId { get; set; }
        public string InstId { get; set; }
        public string FeeId { get { return "00000000-0000-0000-0000-000000000001"; } }
        public string FeeName { get { return "Fees Receivable"; } }
        public decimal Amount { get; set; }
    }
}
