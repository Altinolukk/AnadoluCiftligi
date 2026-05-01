using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Events;
using UnityEngine;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// TEMPORARY DEBUG component. Subscribes to <see cref="AnimalProductHarvestedEvent"/>
    /// and pays out <c>product.BasePrice * amount</c> in the configured currency.
    /// This stand-in keeps gameplay testable until the Market system (FAZ 3.1)
    /// takes over the sale flow. Remove this component from the scene then.
    /// </summary>
    [DisallowMultipleComponent]
    public class AnimalProductPayoutStub : MonoBehaviour
    {
        [SerializeField] private string currencyId = CurrencyIds.Gold;

        [Tooltip("Multiplier applied on top of product.BasePrice. Useful for tuning during early playtests.")]
        [SerializeField, Min(0f)] private float payoutMultiplier = 1f;

        private void OnEnable()
        {
            EventBus.Subscribe<AnimalProductHarvestedEvent>(OnHarvested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AnimalProductHarvestedEvent>(OnHarvested);
        }

        private void OnHarvested(AnimalProductHarvestedEvent e)
        {
            if (e.Product == null || e.Amount <= 0 || !CurrencyService.HasInstance)
            {
                return;
            }

            long basePayout = e.Product.BasePrice * e.Amount;
            long scaled = (long)(basePayout * payoutMultiplier);
            if (scaled <= 0)
            {
                return;
            }

            CurrencyService.Instance.Add(currencyId, scaled);
        }
    }
}
