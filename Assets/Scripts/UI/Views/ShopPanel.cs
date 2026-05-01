using AnadoluCiftligi.Animals;
using UnityEngine;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Optional auto-populator for the shop. Iterates the catalog at Start,
    /// instantiates one <see cref="ShopButton"/> prefab per tier-1 animal,
    /// and binds each button to its <see cref="AnimalData"/>. Set
    /// <c>autoPopulate</c> to false if you'd rather author shop buttons manually.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShopPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AnimalCatalog catalog;
        [SerializeField] private ShopButton buttonPrefab;
        [Tooltip("Parent transform for instantiated buttons. If null, this GameObject's transform is used.")]
        [SerializeField] private Transform buttonContainer;

        [Header("Behavior")]
        [Tooltip("If true, buttons are auto-spawned in Start. Disable to manage children manually.")]
        [SerializeField] private bool autoPopulate = true;
        [Tooltip("Only animals at this tier are eligible for shop entries (tier 1 = base purchasable; higher tiers come from merging).")]
        [SerializeField, Min(1)] private int eligibleTier = 1;

        private void Start()
        {
            if (autoPopulate)
            {
                Populate();
            }
        }

        public void Populate()
        {
            if (catalog == null || buttonPrefab == null)
            {
                Debug.LogWarning("[ShopPanel] catalog or buttonPrefab reference is missing. Populate skipped.");
                return;
            }

            Transform parent = buttonContainer != null ? buttonContainer : transform;

            // Clear any previously spawned children. Manual children remain
            // intact only if a different buttonContainer is configured.
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                Destroy(child.gameObject);
            }

            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                AnimalData entry = catalog.Entries[i];
                if (entry == null || entry.Tier != eligibleTier)
                {
                    continue;
                }

                ShopButton button = Instantiate(buttonPrefab, parent);
                button.name = $"ShopButton_{entry.Id}";
                button.SetAnimalData(entry);
            }
        }
    }
}
