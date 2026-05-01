using AnadoluCiftligi.Events;
using AnadoluCiftligi.Save;
using UnityEngine;

namespace AnadoluCiftligi.Currency
{
    /// <summary>
    /// Scene-bound singleton facade over <see cref="CurrencyWallet"/>. Owns
    /// the wallet lifecycle, registers as <see cref="ISavable"/>, and publishes
    /// <see cref="CurrencyChangedEvent"/> after every successful mutation.
    /// </summary>
    [DisallowMultipleComponent]
    public class CurrencyService : MonoBehaviour, ISavable
    {
        public const string SaveKeyValue = "currency";

        private static CurrencyService instance;

        public static CurrencyService Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[CurrencyService] Instance accessed but no CurrencyService exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("Configuration")]
        [SerializeField] private CurrencyConfig config;

        private CurrencyWallet wallet;

        public string SaveKey => SaveKeyValue;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[CurrencyService] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (config == null)
            {
                Debug.LogError("[CurrencyService] CurrencyConfig reference is missing. Assign it in the inspector.");
            }

            wallet = new CurrencyWallet(config);
            SaveSystem.Register(this);
        }

        private void OnDestroy()
        {
            SaveSystem.Unregister(this);
            if (instance == this)
            {
                instance = null;
            }
        }

        public long Get(string currencyId)
        {
            return wallet != null ? wallet.Get(currencyId) : 0;
        }

        public bool Has(string currencyId, long amount)
        {
            return wallet != null && wallet.Has(currencyId, amount);
        }

        /// <summary>
        /// Adds a positive amount to a currency and publishes a
        /// <see cref="CurrencyChangedEvent"/>. No-op for non-positive amounts.
        /// </summary>
        public void Add(string currencyId, long amount)
        {
            if (wallet == null || string.IsNullOrEmpty(currencyId) || amount <= 0)
            {
                return;
            }

            long previous = wallet.Get(currencyId);
            long updated = wallet.Add(currencyId, amount);
            long delta = updated - previous;
            EventBus.Publish(new CurrencyChangedEvent(currencyId, updated, delta));
        }

        /// <summary>
        /// Attempts to deduct the cost. Returns true and publishes
        /// <see cref="CurrencyChangedEvent"/> on success; returns false and
        /// publishes <see cref="CurrencySpendFailedEvent"/> on insufficient funds.
        /// </summary>
        public bool TrySpend(string currencyId, long cost)
        {
            if (wallet == null || string.IsNullOrEmpty(currencyId))
            {
                return false;
            }
            if (cost <= 0)
            {
                return true;
            }

            long available = wallet.Get(currencyId);
            if (!wallet.TrySpend(currencyId, cost))
            {
                EventBus.Publish(new CurrencySpendFailedEvent(currencyId, cost, available));
                return false;
            }

            long updated = wallet.Get(currencyId);
            EventBus.Publish(new CurrencyChangedEvent(currencyId, updated, -cost));
            return true;
        }

        public string CaptureState()
        {
            if (wallet == null)
            {
                return string.Empty;
            }
            return JsonUtility.ToJson(wallet.Snapshot());
        }

        public void RestoreState(string json)
        {
            if (wallet == null || string.IsNullOrEmpty(json))
            {
                return;
            }

            CurrencyWalletData data = JsonUtility.FromJson<CurrencyWalletData>(json);
            if (data == null)
            {
                return;
            }

            wallet.Restore(data);

            // Notify any subscribers of the restored balances so UI re-renders.
            for (int i = 0; i < data.Entries.Count; i++)
            {
                CurrencyEntry entry = data.Entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }
                EventBus.Publish(new CurrencyChangedEvent(entry.Id, wallet.Get(entry.Id), 0));
            }
        }
    }
}
