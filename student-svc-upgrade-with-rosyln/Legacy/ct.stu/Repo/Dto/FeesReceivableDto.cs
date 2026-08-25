using System.Collections.Generic;

namespace Repo.Dto
{
    public class FeesReceivableDto
    {
        public string InstId { get; set; }
        public string Id { get; set; }
        public string FeeId { get; set; }
        public string FeeName { get; set; }
        public string TxId { get; set; }
        public decimal Amount { get; set; }
    }

    public class FeesReceivableStatusDto
    {
        public string Id { get; set; }
        public string TxId { get; set; }
    }

    public class FeesReceivableStatusBeforeTxDto
    {
        public string InstId { get; set; }
        public string FeeId { get; set; }
        public List<string> StudentIds { get; set; }
        public string Status { get; set; }
    }

    public class FeesReceivableForSpanningFeeDto
    {
        public string InstId { get; set; }
        public string FeeId { get; set; }
        public string FeeName { get; set; }
        public List<string> StudentIds { get; set; }
        public decimal Amount { get; set; }
    }

     public class OpeningBalanceDto
    {
        public string InstId { get; set; }
        public string Id { get; set; }
        public decimal Amount { get; set; }
    }
}