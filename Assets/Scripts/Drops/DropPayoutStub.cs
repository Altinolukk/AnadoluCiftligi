using AnadoluCiftligi.Currency;
using AnadoluCiftligi.Events;
using UnityEngine;

namespace AnadoluCiftligi.Drops
{
    /// <summary>
    /// TEMPORARY DEBUG component. Subscribes to <see cref="ProductDropCollectedEvent"/>
    /// and pays out <c>product.BasePrice * amount</c> in the configured currency.
    /// Replaces <c>AnimalProductPayoutStub</c> now that drops mediate the harvest
    /// visual flow. Both stubs will be removed when the Market system lands in FAZ 3.1.
    /// </summary>
    [DisallowMultipleComponent]
    public class DropPayoutStub : MonoBehaviour
    {
        [SerializeField] private string currencyId = CurrencyIds.Gold;

        [Tooltip("Multiplier applied on top of product.BasePrice. Useful for tuning during early playtests.")]
        [SerializeField, Min(0f)] private float payoutMultiplier = 1f;

        private void OnEnable()
        {
            EventBus.Subscribe<ProductDropCollectedEvent>(OnCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ProductDropCollectedEvent>(OnCollected);
        }

        private void OnCollected(ProductDropCollectedEvent e)
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
