using ct._Core.Security;
using ct._Core.Util;
using Fees.Repo.Resource;
using Marten;
using Marten.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Fees.Repo.Db.Command
{
    /// <summary>
    /// Fee-receivable / opening-balance writes. Moved here verbatim (logic unchanged) from
    /// Student/Repo/Db/Command/StudentCmd.cs — the only change is that it now operates on
    /// first-class FeesReceivable/OpeningBalance documents in the Fees module's own database,
    /// keyed by StudentId, instead of loading and mutating a Student document.
    /// </summary>
    public class FeesCmd
    {
        readonly IAsyncDocumentSession _session;
        readonly IUserContext _uc;

        public FeesCmd(IUserContext uc, IAsyncDocumentSession session)
        {
            _uc = uc.Current();
            _session = session;
        }

        public FeesReceivable UpdateFeesReceivable(string studentId, string instId, string feeId, string feeName, string txId, decimal amount)
        {
            var existing = _session.Query<FeesReceivable>()
                .Where(o => o.StudentId == studentId && o.TxId == txId)
                .ToList().FirstOrDefault();
            if (existing != null) return existing;

            var dto = new FeesReceivable
            {
                StudentId = studentId,
                InstId = instId,
                FeeId = feeId,
                FeeName = feeName,
                TxId = txId,
                Amount = amount,
                TxStatus = TxStatusEnum.Active
            };
            _session.StoreAsync(dto).Wait();
            _session.SaveChangesAsync().Wait();
            return dto;
        }

        public FeesReceivable UpdateFeesReceivableStatus(string studentId, string txId, string instId, string feeId, string feeName, decimal amount)
        {
            var existing = _session.Query<FeesReceivable>()
                .Where(o => o.StudentId == studentId && o.TxId == txId)
                .ToList().FirstOrDefault();
            if (existing == null) throw new ObjectNotFoundException();

            existing.TxStatus = TxStatusEnum.Disabled;

            var dto = new FeesReceivable
            {
                StudentId = studentId,
                InstId = instId,
                FeeId = feeId,
                FeeName = feeName,
                TxId = string.Empty,
                Amount = amount,
                TxStatus = TxStatusEnum.Active
            };
            _session.StoreAsync(dto).Wait();
            _session.SaveChangesAsync().Wait();
            return dto;
        }

        public void UpdateFeesReceivableForSpanningFee(string instId, string feeId, string feeName, decimal amount, List<string> studentIds)
        {
            foreach (var studentId in studentIds)
            {
                var existing = _session.Query<FeesReceivable>()
                    .Where(o => o.StudentId == studentId && o.FeeId == feeId && o.InstId == instId)
                    .ToList().FirstOrDefault();

                if (existing != null)
                {
                    existing.Amount = amount;
                    existing.TxStatus = TxStatusEnum.Active;
                }
                else
                {
                    var dto = new FeesReceivable
                    {
                        StudentId = studentId,
                        InstId = instId,
                        FeeId = feeId,
                        FeeName = feeName,
                        TxId = string.Empty,
                        Amount = amount,
                        TxStatus = TxStatusEnum.Active
                    };
                    _session.StoreAsync(dto).Wait();
                }
            }
            _session.SaveChangesAsync().Wait();
        }

        public void UpdateFeesReceivableStatusBeforeTx(string instId, string feeId, string status, List<string> studentIds)
        {
            foreach (var studentId in studentIds)
            {
                var existing = _session.Query<FeesReceivable>()
                    .Where(o => o.StudentId == studentId && o.FeeId == feeId && o.InstId == instId)
                    .ToList().FirstOrDefault();
                if (existing != null)
                    existing.TxStatus = status == "Active" ? TxStatusEnum.Active : TxStatusEnum.Disabled;
            }
            _session.SaveChangesAsync().Wait();
        }

        public OpeningBalance UpdateOpeningBalance(string studentId, string instId, decimal amount)
        {
            var existing = _session.Query<OpeningBalance>()
                .Where(o => o.StudentId == studentId && o.InstId == instId)
                .ToList().FirstOrDefault();

            if (existing == null)
            {
                existing = new OpeningBalance { StudentId = studentId, InstId = instId, Amount = amount };
                _session.StoreAsync(existing).Wait();
            }
            else
            {
                existing.Amount = amount;
            }
            _session.SaveChangesAsync().Wait();
            return existing;
        }
    }
}
