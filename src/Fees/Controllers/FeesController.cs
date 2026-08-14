using Fees.Repo.Db.Command;
using Fees.Repo.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Fees.Controllers
{
    /// <summary>
    /// Fee-receivable / opening-balance endpoints. Relocated from StudentController —
    /// same routes minus the "student/" prefix, same request/response shapes, now writing
    /// to the Fees module's own database instead of reaching into Student's.
    /// </summary>
    [Route("api/fees/[controller]")]
    [ApiController]
    public class FeesController : ControllerBase
    {
        readonly FeesCmd _feesCmd;

        public FeesController(FeesCmd feesCmd)
        {
            _feesCmd = feesCmd;
        }

        [HttpPost("feesReceivable")]
        public IActionResult UpdateFeesReceivable([FromBody] FeesReceivableDto dto)
        {
            var result = _feesCmd.UpdateFeesReceivable(dto.Id, dto.InstId, dto.FeeId, dto.FeeName, dto.TxId, dto.Amount);
            return Ok(result);
        }

        [HttpPost("feesReceivableUpdate")]
        public IActionResult UpdateFeesReceivableStatus([FromBody] FeesReceivableDto dto)
        {
            var result = _feesCmd.UpdateFeesReceivableStatus(dto.Id, dto.TxId, dto.InstId, dto.FeeId, dto.FeeName, dto.Amount);
            return Ok(result);
        }

        [HttpPost("feesReceivableForSpanningFee")]
        public IActionResult UpdateFeesReceivableForSpanningFee([FromBody] FeesReceivableForSpanningFeeDto dto)
        {
            _feesCmd.UpdateFeesReceivableForSpanningFee(dto.InstId, dto.FeeId, dto.FeeName, dto.Amount, dto.StudentIds);
            return Ok();
        }

        [HttpPost("feesReceivableStatusUpdate")]
        public IActionResult UpdateFeesReceivableStatusBeforeTx([FromBody] FeesReceivableStatusBeforeTxDto dto)
        {
            _feesCmd.UpdateFeesReceivableStatusBeforeTx(dto.InstId, dto.FeeId, dto.Status, dto.StudentIds);
            return Ok();
        }

        [HttpPost("openingBalance")]
        public IActionResult UpdateOpeningBalance([FromBody] OpeningBalanceDto dto)
        {
            var result = _feesCmd.UpdateOpeningBalance(dto.Id, dto.InstId, dto.Amount);
            return Ok(result);
        }
    }
}
