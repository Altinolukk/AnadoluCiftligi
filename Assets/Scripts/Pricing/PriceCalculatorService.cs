using System;
using System.Collections.Generic;
using AnadoluCiftligi.Animals;
using AnadoluCiftligi.Products;
using UnityEngine;

namespace AnadoluCiftligi.Pricing
{
    /// <summary>
    /// Scene singleton that computes dynamic prices for purchases and sales.
    /// ShopService and MarketService delegate here when an instance exists,
    /// otherwise they fall back to base prices. Purchase cost grows by
    /// <c>purchaseGrowthFactor ^ owned</c>, where <c>owned</c> is the count
    /// of currently active animals matching the same Id.
    ///
    /// Known limitation: count is based on currently active animals. After a
    /// merge consumes two T1 animals, T1 cost briefly drops. A "totalPurchased"
    /// save-backed counter is the proper fix and can be introduced later
    /// without changing the public API.
    /// </summary>
    [DisallowMultipleComponent]
    public class PriceCalculatorService : MonoBehaviour
    {
        private static PriceCalculatorService instance;

        public static PriceCalculatorService Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[PriceCalculatorService] Instance accessed but no PriceCalculatorService exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("Configuration")]
        [SerializeField] private PriceConfig config;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[PriceCalculatorService] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (config == null)
            {
                Debug.LogWarning("[PriceCalculatorService] PriceConfig is missing. Falling back to default constants (growth=1.15, saleMultiplier=1).");
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public long GetPurchaseCost(AnimalData data)
        {
            if (data == null)
            {
                return 0;
            }

            long baseCost = data.PurchaseCost;
            if (baseCost <= 0)
            {
                return 0;
            }

            int owned = CountActive(data);
            float growth = config != null ? config.PurchaseGrowthFactor : 1.15f;
            if (growth <= 1f)
            {
                return baseCost;
            }

            double scaled = baseCost * Math.Pow(growth, owned);
            if (scaled >= long.MaxValue)
            {
                return long.MaxValue;
            }
            if (scaled <= 0)
            {
                return 0;
            }
            return (long)scaled;
        }

        public long GetSalePrice(ProductDefinition product)
        {
            if (product == null)
            {
                return 0;
            }

            long basePrice = product.BasePrice;
            float multiplier = config != null ? config.SalePriceMultiplier : 1f;
            if (multiplier <= 0f)
            {
                return 0;
            }
            float scaled = basePrice * multiplier;
            return scaled <= 0f ? 0 : (long)scaled;
        }

        public int CountActive(AnimalData data)
        {
            if (data == null || !AnimalSpawner.HasInstance)
            {
                return 0;
            }

            IReadOnlyList<Animal> actives = AnimalSpawner.Instance.ActiveAnimals;
            int count = 0;
            for (int i = 0; i < actives.Count; i++)
            {
                Animal animal = actives[i];
                if (animal != null && animal.Data != null && animal.Data.Id == data.Id)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
