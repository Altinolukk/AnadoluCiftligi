using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// Raised when an <see cref="Animal"/> is harvested by player click and
    /// emits its accumulated production. Listeners (Market, ProductDrop spawner)
    /// react without having to reach back into the Animal instance.
    /// </summary>
    public readonly struct AnimalProductHarvestedEvent : IGameEvent
    {
        public readonly Animal Source;
        public readonly AnimalData Data;
        public readonly ProductDefinition Product;
        public readonly int Amount;

        public AnimalProductHarvestedEvent(Animal source, AnimalData data, ProductDefinition product, int amount)
        {
            Source = source;
            Data = data;
            Product = product;
            Amount = amount;
        }
    }

    /// <summary>
    /// Raised whenever an <see cref="Animal"/>'s pending product count changes
    /// (production tick or harvest). UI can listen to drive ready-state visuals.
    /// </summary>
    public readonly struct AnimalPendingChangedEvent : IGameEvent
    {
        public readonly Animal Source;
        public readonly int Pending;
        public readonly int MaxPending;

        public AnimalPendingChangedEvent(Animal source, int pending, int maxPending)
        {
            Source = source;
            Pending = pending;
            MaxPending = maxPending;
        }
    }
}
