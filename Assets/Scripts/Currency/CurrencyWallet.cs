using System;
using System.Collections.Generic;

namespace AnadoluCiftligi.Currency
{
    /// <summary>
    /// Pure-C# runtime state for currency balances. Has no MonoBehaviour or
    /// SaveSystem coupling on its own; <see cref="CurrencyService"/> owns it
    /// and handles persistence + event publishing.
    /// </summary>
    public class CurrencyWallet
    {
        private readonly Dictionary<string, long> balances = new Dictionary<string, long>();

        /// <summary>
        /// Seeds wallet with default starting amounts from the config. If no
        /// config is provided, the wallet starts empty and unknown currencies
        /// resolve to 0.
        /// </summary>
        public CurrencyWallet(CurrencyConfig config)
        {
            if (config == null)
            {
                return;
            }

            for (int i = 0; i < config.Currencies.Count; i++)
            {
                CurrencyDefinition def = config.Currencies[i];
                if (def == null || string.IsNullOrEmpty(def.Id))
                {
                    continue;
                }
                balances[def.Id] = def.DefaultStartingAmount;
            }
        }

        public long Get(string currencyId)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                return 0;
            }
            return balances.TryGetValue(currencyId, out long value) ? value : 0;
        }

        public bool Has(string currencyId, long amount)
        {
            if (amount <= 0)
            {
                return true;
            }
            return Get(currencyId) >= amount;
        }

        /// <summary>
        /// Adds a positive amount to a currency. Negative amounts are ignored;
        /// use <see cref="TrySpend"/> for deductions.
        /// </summary>
        public long Add(string currencyId, long amount)
        {
            if (string.IsNullOrEmpty(currencyId) || amount <= 0)
            {
                return Get(currencyId);
            }

            long current = Get(currencyId);
            long updated = SafeAdd(current, amount);
            balances[currencyId] = updated;
            return updated;
        }

        /// <summary>
        /// Atomically deducts the cost if the wallet can afford it. Returns
        /// false without mutating state if the balance is insufficient.
        /// </summary>
        public bool TrySpend(string currencyId, long cost)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                return false;
            }
            if (cost <= 0)
            {
                return true;
            }

            long current = Get(currencyId);
            if (current < cost)
            {
                return false;
            }

            balances[currencyId] = current - cost;
            return true;
        }

        public CurrencyWalletData Snapshot()
        {
            CurrencyWalletData data = new CurrencyWalletData();
            foreach (KeyValuePair<string, long> pair in balances)
            {
                data.Entries.Add(new CurrencyEntry { Id = pair.Key, Amount = pair.Value });
            }
            return data;
        }

        /// <summary>
        /// Merges a previously captured snapshot into the wallet. Currencies
        /// missing from the snapshot keep their current (default) value, so
        /// new currencies introduced post-release still appear with their
        /// configured starting amount.
        /// </summary>
        public void Restore(CurrencyWalletData data)
        {
            if (data == null || data.Entries == null)
            {
                return;
            }

            for (int i = 0; i < data.Entries.Count; i++)
            {
                CurrencyEntry entry = data.Entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }
                balances[entry.Id] = entry.Amount;
            }
        }

        private static long SafeAdd(long a, long b)
        {
            // Saturate at long.MaxValue rather than overflowing, even though
            // reaching it requires balances no idle game realistically hits.
            try
            {
                return checked(a + b);
            }
            catch (OverflowException)
            {
                return long.MaxValue;
            }
        }
    }

    [Serializable]
    public class CurrencyWalletData
    {
        public List<CurrencyEntry> Entries = new List<CurrencyEntry>();
    }

    [Serializable]
    public class CurrencyEntry
    {
        public string Id = string.Empty;
        public long Amount;
    }
}
