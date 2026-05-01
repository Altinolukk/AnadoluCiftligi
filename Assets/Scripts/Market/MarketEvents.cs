using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;

namespace AnadoluCiftligi.Market
{
    /// <summary>
    /// Raised by <see cref="MarketService"/> after a successful sale. Listeners
    /// can drive sale feedback (toast, sound, "+N gold" floater) or analytics.
    /// </summary>
    public readonly struct ProductSoldEvent : IGameEvent
    {
        public readonly ProductDefinition Product;
        public readonly int Amount;
        public readonly string CurrencyId;
        public readonly long UnitPrice;
        public readonly long TotalPaid;

        public ProductSoldEvent(ProductDefinition product, int amount, string currencyId, long unitPrice, long totalPaid)
        {
            Product = product;
            Amount = amount;
            CurrencyId = currencyId;
            UnitPrice = unitPrice;
            TotalPaid = totalPaid;
        }
    }
}
