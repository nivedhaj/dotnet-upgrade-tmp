namespace Ct.Fee.Application;

public sealed class FeeCalculationService
{
    public decimal CalculateAmount(IEnumerable<decimal> installments, IEnumerable<decimal> fines, IEnumerable<decimal> discounts, IEnumerable<decimal> adjustments)
    {
        var subtotal = installments.Sum() + fines.Sum() + adjustments.Sum() - discounts.Sum();
        return decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
    }
}
