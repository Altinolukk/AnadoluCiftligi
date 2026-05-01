using AnadoluCiftligi.Animals;
using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Single shop entry bound to one <see cref="AnimalData"/>. Displays
    /// name, dynamic price, and icon. Refreshes when the matching currency
    /// changes (interactable state) or when an animal of the same kind is
    /// spawned/despawned (price label, since dynamic cost depends on count).
    /// Click triggers <see cref="ShopService.TryPurchase"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ShopButton : UIView
    {
        [Header("Binding")]
        [SerializeField] private AnimalData animalData;

        [Header("Display References")]
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI priceLabel;
        [SerializeField] private Image iconImage;

        [Header("Display")]
        [Tooltip("Format applied to the price label. {0} is the cost, {1} is the currency id.")]
        [SerializeField] private string priceFormat = "{0}";
        [Tooltip("Text shown when cost is 0.")]
        [SerializeField] private string freeLabel = "Bedava";

        private Button button;

        public AnimalData AnimalData => animalData;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void SetAnimalData(AnimalData data)
        {
            animalData = data;
            Refresh();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
            Refresh();
        }

        protected override void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
            base.OnDisable();
        }

        protected override void BindEvents()
        {
            Bind<CurrencyChangedEvent>(OnCurrencyChanged);
            Bind<AnimalSpawnedEvent>(OnAnimalSpawned);
            Bind<AnimalDespawnedEvent>(OnAnimalDespawned);
        }

        private void OnClicked()
        {
            if (animalData == null)
            {
                return;
            }
            if (!ShopService.HasInstance)
            {
                Debug.LogWarning("[ShopButton] ShopService not present in scene.");
                return;
            }
            ShopService.Instance.TryPurchase(animalData);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent e)
        {
            if (animalData != null && e.CurrencyId == animalData.PurchaseCurrencyId)
            {
                UpdateInteractable();
            }
        }

        private void OnAnimalSpawned(AnimalSpawnedEvent e)
        {
            if (animalData != null && e.Animal != null && e.Animal.Data != null && e.Animal.Data.Id == animalData.Id)
            {
                Refresh();
            }
        }

        private void OnAnimalDespawned(AnimalDespawnedEvent e)
        {
            if (animalData != null && e.Animal != null && e.Animal.Data != null && e.Animal.Data.Id == animalData.Id)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (animalData == null)
            {
                if (nameLabel != null) nameLabel.text = "?";
                if (priceLabel != null) priceLabel.text = "-";
                if (iconImage != null) iconImage.enabled = false;
                if (button != null) button.interactable = false;
                return;
            }

            if (nameLabel != null)
            {
                nameLabel.text = animalData.DisplayName;
            }

            long cost = ShopService.HasInstance
                ? ShopService.Instance.ComputeCost(animalData)
                : animalData.PurchaseCost;

            if (priceLabel != null)
            {
                priceLabel.text = cost <= 0
                    ? freeLabel
                    : string.Format(priceFormat, cost, animalData.PurchaseCurrencyId);
            }

            if (iconImage != null)
            {
                iconImage.sprite = animalData.Sprite;
                iconImage.enabled = animalData.Sprite != null;
            }

            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            if (button == null || animalData == null)
            {
                return;
            }

            if (ShopService.HasInstance)
            {
                button.interactable = ShopService.Instance.CanAfford(animalData);
            }
            else if (CurrencyService.HasInstance)
            {
                button.interactable = CurrencyService.Instance.Has(animalData.PurchaseCurrencyId, animalData.PurchaseCost);
            }
        }
    }
}
