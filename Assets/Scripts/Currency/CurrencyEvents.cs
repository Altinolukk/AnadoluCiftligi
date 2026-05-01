using AnadoluCiftligi.Events;

namespace AnadoluCiftligi.Currency
{
    /// <summary>
    /// Raised after any successful change to a currency balance.
    /// </summary>
    public readonly struct CurrencyChangedEvent : IGameEvent
    {
        public readonly string CurrencyId;
        public readonly long NewBalance;
        public readonly long Delta;

        public CurrencyChangedEvent(string currencyId, long newBalance, long delta)
        {
            CurrencyId = currencyId;
            NewBalance = newBalance;
            Delta = delta;
        }
    }

    /// <summary>
    /// Raised when a TrySpend call fails because of insufficient balance.
    /// UI layers can listen to surface a "not enough" feedback (toast, shake, etc.).
    /// </summary>
    public readonly struct CurrencySpendFailedEvent : IGameEvent
    {
        public readonly string CurrencyId;
        public readonly long Required;
        public readonly long Available;

        public CurrencySpendFailedEvent(string currencyId, long required, long available)
        {
            CurrencyId = currencyId;
            Required = required;
            Available = available;
        }
    }
}
