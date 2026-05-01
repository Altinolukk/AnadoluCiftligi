using AnadoluCiftligi.Animals;
using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Pricing;
using UnityEngine;

namespace AnadoluCiftligi.Shop
{
    /// <summary>
    /// Scene singleton coordinating purchase flow. Atomic semantics:
    /// currency is deducted first; if the spawn step fails the deducted
    /// amount is refunded so the wallet stays consistent. Free items
    /// (Cost == 0) skip the currency interaction entirely.
    /// Per-purchase price scaling delegates to <see cref="PriceCalculatorService"/>
    /// when present; otherwise falls back to <see cref="AnimalData.PurchaseCost"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopService : MonoBehaviour
    {
        private static ShopService instance;

        public static ShopService Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[ShopService] Instance accessed but no ShopService exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("Configuration")]
        [Tooltip("If true, every successful purchase logs a one-line summary to Console.")]
        [SerializeField] private bool logPurchases = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[ShopService] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Returns the current per-purchase cost for the given animal data.
        /// Delegates to <see cref="PriceCalculatorService"/> when present so
        /// the n-th purchase of the same animal costs more than the first.
        /// </summary>
        public long ComputeCost(AnimalData data)
        {
            if (data == null)
            {
                return 0;
            }
            if (PriceCalculatorService.HasInstance)
            {
                return PriceCalculatorService.Instance.GetPurchaseCost(data);
            }
            return data.PurchaseCost;
        }

        public bool CanAfford(AnimalData data)
        {
            if (data == null)
            {
                return false;
            }
            long cost = ComputeCost(data);
            if (cost <= 0)
            {
                return true;
            }
            return CurrencyService.HasInstance && CurrencyService.Instance.Has(data.PurchaseCurrencyId, cost);
        }

        public bool TryPurchase(AnimalData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[ShopService] TryPurchase called with null AnimalData.");
                return false;
            }

            if (!AnimalSpawner.HasInstance)
            {
                Debug.LogError("[ShopService] AnimalSpawner is not present in the scene; cannot complete purchase.");
                return false;
            }

            string currencyId = data.PurchaseCurrencyId;
            long cost = ComputeCost(data);

            if (cost > 0)
            {
                if (!CurrencyService.HasInstance)
                {
                    Debug.LogError("[ShopService] CurrencyService is not present in the scene; cannot complete purchase.");
                    return false;
                }

                long available = CurrencyService.Instance.Get(currencyId);
                if (!CurrencyService.Instance.TrySpend(currencyId, cost))
                {
                    EventBus.Publish(new ShopPurchaseFailedEvent(data, currencyId, cost, available));
                    return false;
                }
            }

            Animal spawned = AnimalSpawner.Instance.Spawn(data);
            if (spawned == null)
            {
                if (cost > 0 && CurrencyService.HasInstance)
                {
                    CurrencyService.Instance.Add(currencyId, cost);
                }
                Debug.LogError($"[ShopService] Spawn failed for '{data.Id}'. Currency refunded.");
                return false;
            }

            EventBus.Publish(new ShopPurchasedEvent(data, spawned, currencyId, cost));

            if (logPurchases)
            {
                Debug.Log($"[ShopService] Purchased '{data.Id}' for {cost} {currencyId}.");
            }
            return true;
        }
    }
}
