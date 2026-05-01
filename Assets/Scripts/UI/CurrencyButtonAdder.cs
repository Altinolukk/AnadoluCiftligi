using AnadoluCiftligi.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Debug helper: adds a fixed amount of a chosen currency on each click.
    /// Wires its own listener to the sibling <see cref="Button"/>. Used to
    /// drive currency changes before the real Shop/Animal systems land.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CurrencyButtonAdder : MonoBehaviour
    {
        [SerializeField] private string currencyId = CurrencyIds.Gold;
        [SerializeField] private long amount = 1;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (!CurrencyService.HasInstance || amount <= 0)
            {
                return;
            }
            CurrencyService.Instance.Add(currencyId, amount);
        }
    }
}
