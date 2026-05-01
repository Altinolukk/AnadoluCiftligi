using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Products;
using UnityEngine;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// ScriptableObject describing a single producer entity (animal or tree)
    /// at a specific tier. Designers create one asset per tier per species,
    /// e.g. Chicken_T1, Chicken_T2, Chicken_T3. Merging two tier-N producers
    /// yields one tier-(N+1) producer; the result is found via <see cref="NextTier"/>.
    /// Production stats and visuals are stored per-tier directly on the asset,
    /// so designers control the merge reward curve without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Animals/Animal Data", fileName = "Animal_New")]
    public class AnimalData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable identifier including tier suffix, e.g. 'chicken_t1'. Lowercase ASCII; never change after release without save migration.")]
        [SerializeField] private string id = string.Empty;

        [Tooltip("Display name for UI, e.g. 'Tavuk'. Localization layer will translate this later.")]
        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private AnimalKind kind = AnimalKind.Animal;

        [SerializeField] private Sprite sprite;

        [SerializeField, TextArea(2, 4)] private string description = string.Empty;

        [Header("Tier")]
        [Tooltip("1-based tier index. Tier 1 is purchased from the shop; higher tiers are obtained by merging two tier-N producers.")]
        [SerializeField, Min(1)] private int tier = 1;

        [Tooltip("Result of merging two of this asset. Leave empty on the highest tier.")]
        [SerializeField] private AnimalData nextTier;

        [Header("Production")]
        [SerializeField] private ProductDefinition product;

        [Tooltip("Seconds between production cycles.")]
        [SerializeField, Min(0.1f)] private float productionInterval = 5f;

        [Tooltip("How many products are emitted per cycle. Use higher tiers' productionAmount/productionInterval to encode the merge multiplier (e.g. 3x).")]
        [SerializeField, Min(1)] private int productionAmount = 1;

        [Header("Purchase")]
        [Tooltip("Currency used to buy this producer from the shop. Match a CurrencyDefinition.Id (use CurrencyIds constants).")]
        [SerializeField] private string purchaseCurrencyId = CurrencyIds.Gold;

        [Tooltip("Base purchase cost. Per-instance scaling (e.g. each next chicken costs more) lives in PriceCalculator (FAZ 3.3).")]
        [SerializeField, Min(0)] private long purchaseCost = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public AnimalKind Kind => kind;
        public Sprite Sprite => sprite;
        public string Description => description;

        public int Tier => tier;
        public AnimalData NextTier => nextTier;
        public bool HasNextTier => nextTier != null;

        public ProductDefinition Product => product;
        public float ProductionInterval => productionInterval;
        public int ProductionAmount => productionAmount;

        public string PurchaseCurrencyId => purchaseCurrencyId;
        public long PurchaseCost => purchaseCost;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (nextTier == this)
            {
                Debug.LogError($"[AnimalData] '{name}' references itself as NextTier. Clearing.");
                nextTier = null;
            }

            if (nextTier != null && nextTier.tier <= tier)
            {
                Debug.LogWarning($"[AnimalData] '{name}' (tier {tier}) points to NextTier '{nextTier.name}' (tier {nextTier.tier}). NextTier should be strictly higher.");
            }

            if (productionInterval < 0.1f)
            {
                productionInterval = 0.1f;
            }

            if (productionAmount < 1)
            {
                productionAmount = 1;
            }
        }
#endif
    }
}
