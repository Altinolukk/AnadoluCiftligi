using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;

namespace AnadoluCiftligi.Drops
{
    /// <summary>
    /// Raised by <see cref="ProductDrop"/> when its travel animation reaches
    /// the target position and the carried units are considered "collected".
    /// Listeners (Market in FAZ 3.1, currently <see cref="DropPayoutStub"/>)
    /// react to credit the player.
    /// </summary>
    public readonly struct ProductDropCollectedEvent : IGameEvent
    {
        public readonly ProductDefinition Product;
        public readonly int Amount;

        public ProductDropCollectedEvent(ProductDefinition product, int amount)
        {
            Product = product;
            Amount = amount;
        }
    }
}
