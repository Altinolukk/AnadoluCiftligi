using AnadoluCiftligi.Animals;
using AnadoluCiftligi.Events;

namespace AnadoluCiftligi.Shop
{
    /// <summary>
    /// Raised after a successful shop purchase: currency was deducted and a
    /// new animal instance was spawned.
    /// </summary>
    public readonly struct ShopPurchasedEvent : IGameEvent
    {
        public readonly AnimalData Data;
        public readonly Animal Animal;
        public readonly string CurrencyId;
        public readonly long Cost;

        public ShopPurchasedEvent(AnimalData data, Animal animal, string currencyId, long cost)
        {
            Data = data;
            Animal = animal;
            CurrencyId = currencyId;
            Cost = cost;
        }
    }

    /// <summary>
    /// Raised when a purchase attempt fails — most commonly insufficient
    /// funds. UI can listen to surface a "not enough" feedback (toast, shake).
    /// </summary>
    public readonly struct ShopPurchaseFailedEvent : IGameEvent
    {
        public readonly AnimalData Data;
        public readonly string CurrencyId;
        public readonly long RequiredCost;
        public readonly long Available;

        public ShopPurchaseFailedEvent(AnimalData data, string currencyId, long requiredCost, long available)
        {
            Data = data;
            CurrencyId = currencyId;
            RequiredCost = requiredCost;
            Available = available;
        }
    }
}
