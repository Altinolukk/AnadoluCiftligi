using UnityEngine;

namespace AnadoluCiftligi.Products
{
    /// <summary>
    /// ScriptableObject describing a single product (egg, milk, wool, apple, ...).
    /// Owned by the Products module and referenced by Animals (as production output)
    /// and later by Market (sale price) and Crafting (recipe ingredients).
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Products/Product Definition", fileName = "Product_New")]
    public class ProductDefinition : ScriptableObject
    {
        [Tooltip("Stable identifier. Use lowercase ASCII (e.g., 'egg', 'milk'). Never change after release without a save migration.")]
        [SerializeField] private string id = string.Empty;

        [Tooltip("Display name for UI. Localization layer will translate this later.")]
        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private Sprite sprite;

        [Tooltip("Default sale price used by the Market. Per-shop scaling lives in PriceCalculator (FAZ 3.3).")]
        [SerializeField, Min(0)] private long basePrice = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
        public long BasePrice => basePrice;
    }
}
