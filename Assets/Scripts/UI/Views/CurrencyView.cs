using AnadoluCiftligi.Currency;
using TMPro;
using UnityEngine;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Displays a single currency's balance in a TMP label. Subscribes to
    /// <see cref="CurrencyChangedEvent"/> and updates only when the matching
    /// currency changes — no per-frame polling.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CurrencyView : UIView
    {
        [Header("Binding")]
        [Tooltip("Which currency this view tracks. Match a CurrencyDefinition.Id (use CurrencyIds constants).")]
        [SerializeField] private string currencyId = CurrencyIds.Gold;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI label;

        [Header("Display")]
        [Tooltip("Format string. {0} is the balance, {1} is the currency id.")]
        [SerializeField] private string format = "{0}";

        private void Reset()
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        protected override void BindEvents()
        {
            if (label == null)
            {
                label = GetComponent<TextMeshProUGUI>();
            }

            Bind<CurrencyChangedEvent>(OnCurrencyChanged);

            // Pull current balance immediately to avoid a one-event lag on enable.
            if (CurrencyService.HasInstance)
            {
                Render(CurrencyService.Instance.Get(currencyId));
            }
            else
            {
                Render(0);
            }
        }

        private void OnCurrencyChanged(CurrencyChangedEvent e)
        {
            if (e.CurrencyId == currencyId)
            {
                Render(e.NewBalance);
            }
        }

        private void Render(long amount)
        {
            if (label == null)
            {
                return;
            }
            label.text = string.Format(format, amount, currencyId);
        }
    }
}
