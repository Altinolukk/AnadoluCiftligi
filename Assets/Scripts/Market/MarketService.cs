using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Drops;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Pricing;
using AnadoluCiftligi.Products;
using UnityEngine;

namespace AnadoluCiftligi.Market
{
    /// <summary>
    /// Auto-sells products as they arrive at the drop target. Subscribes to
    /// <see cref="ProductDropCollectedEvent"/>; for each event computes a unit
    /// price (delegating to <see cref="PriceCalculatorService"/> if present,
    /// else using <c>BasePrice * globalPriceMultiplier</c>) and credits the
    /// configured currency. Emits <see cref="ProductSoldEvent"/>.
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

        [Tooltip("Multiplier applied to product.BasePrice when no PriceCalculatorService is present. Ignored when the calculator is in scene (use PriceConfig.salePriceMultiplier instead).")]
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

        public long ComputeUnitPrice(ProductDefinition product)
        {
            if (product == null)
            {
                return 0;
            }

            if (PriceCalculatorService.HasInstance)
            {
                return PriceCalculatorService.Instance.GetSalePrice(product);
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
