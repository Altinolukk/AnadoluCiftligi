using UnityEngine;

namespace AnadoluCiftligi.Pricing
{
    /// <summary>
    /// Tunable pricing constants. Designers can hot-edit growth curves and
    /// sale multipliers without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Pricing/Price Config", fileName = "PriceConfig")]
    public class PriceConfig : ScriptableObject
    {
        [Header("Purchase")]
        [Tooltip("Multiplier applied per existing animal of the same type. 1.15 means each next purchase costs 15% more. Use 1.0 to disable scaling.")]
        [SerializeField, Min(1f)] private float purchaseGrowthFactor = 1.15f;

        [Header("Sale")]
        [Tooltip("Global multiplier on Product.BasePrice. 1.0 = base price; >1 boosts payouts globally. PriceCalculator uses this when computing sale prices for the Market.")]
        [SerializeField, Min(0f)] private float salePriceMultiplier = 1f;

        public float PurchaseGrowthFactor => purchaseGrowthFactor;
        public float SalePriceMultiplier => salePriceMultiplier;
    }
}
