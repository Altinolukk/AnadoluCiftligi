using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Drops;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;
using UnityEngine;

namespace AnadoluCiftligi.Market
{
    /// <summary>
    /// Auto-sells products as they arrive at the drop target. Subscribes to
    /// <see cref="ProductDropCollectedEvent"/>; for each event computes a unit
    /// price and credits the configured currency. Emits <see cref="ProductSoldEvent"/>
    /// so UI and analytics can react. Replaces the temporary payout stubs from
    /// FAZ 2.2/2.4. Dynamic price scaling lands in FAZ 3.3 (PriceCalculator).
    /// </summary>
    [DisallowMultipleComponent]
    public class MarketService : MonoBehaviour
    {
        private static MarketService instance;

        public static MarketService Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[MarketService] Instance accessed but no MarketService exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("Configuration")]
        [Tooltip("Currency credited on every sale. Match a CurrencyDefinition.Id (use CurrencyIds constants).")]
        [SerializeField] private string payoutCurrencyId = CurrencyIds.Gold;

        [Tooltip("Multiplier applied to product.BasePrice. Useful for global tuning during early playtests; PriceCalculator (FAZ 3.3) will replace this with per-product scaling.")]
        [SerializeField, Min(0f)] private float globalPriceMultiplier = 1f;

        [Tooltip("If true, every sale logs a one-line summary to Console. Disable for shipping builds.")]
        [SerializeField] private bool logSales = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[MarketService] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            EventBus.Subscribe<ProductDropCollectedEvent>(OnProductCollected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ProductDropCollectedEvent>(OnProductCollected);
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnProductCollected(ProductDropCollectedEvent e)
        {
            if (e.Product == null || e.Amount <= 0)
            {
                return;
            }

            if (!CurrencyService.HasInstance)
            {
                Debug.LogWarning("[MarketService] CurrencyService not available; sale dropped.");
                return;
            }

            long unitPrice = ComputeUnitPrice(e.Product);
            if (unitPrice <= 0)
            {
                return;
            }

            long total = unitPrice * e.Amount;
            CurrencyService.Instance.Add(payoutCurrencyId, total);
            EventBus.Publish(new ProductSoldEvent(e.Product, e.Amount, payoutCurrencyId, unitPrice, total));

            if (logSales)
            {
                Debug.Log($"[MarketService] Sold {e.Amount} x {e.Product.Id} @ {unitPrice} = {total} {payoutCurrencyId}");
            }
        }

        /// <summary>
        /// Returns the current per-unit sale price for a product. FAZ 3.3 will
        /// override this entry point with dynamic scaling (per-product demand,
        /// season multipliers, etc.) — keep call sites going through here.
        /// </summary>
        public long ComputeUnitPrice(ProductDefinition product)
        {
            if (product == null)
            {
                return 0;
            }
            float scaled = product.BasePrice * globalPriceMultiplier;
            if (scaled <= 0f)
            {
                return 0;
            }
            return (long)scaled;
        }
    }
}
