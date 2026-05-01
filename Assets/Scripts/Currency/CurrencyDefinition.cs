using UnityEngine;

namespace AnadoluCiftligi.Currency
{
    /// <summary>
    /// ScriptableObject describing a single currency. Designers create one asset
    /// per currency (gold, gem, future event tokens) under Assets/ScriptableObjects/Currency/.
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Currency/Currency Definition", fileName = "Currency_New")]
    public class CurrencyDefinition : ScriptableObject
    {
        [Tooltip("Stable identifier used as the wallet key. Use lowercase ASCII (e.g., 'gold', 'gem'). Never change after release without a save migration.")]
        [SerializeField] private string id = string.Empty;

        [Tooltip("Display name for UI (e.g., 'Altin'). Localization layer will translate this later.")]
        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private Sprite icon;

        [Tooltip("Amount granted to a fresh wallet (no save file). Set to 0 for premium currencies.")]
        [SerializeField] private long defaultStartingAmount = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public long DefaultStartingAmount => defaultStartingAmount;
    }
}
