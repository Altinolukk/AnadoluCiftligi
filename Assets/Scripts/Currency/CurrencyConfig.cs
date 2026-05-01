using System.Collections.Generic;
using UnityEngine;

namespace AnadoluCiftligi.Currency
{
    /// <summary>
    /// Container ScriptableObject aggregating every <see cref="CurrencyDefinition"/>
    /// the game knows about. Assigned to <see cref="CurrencyService"/> in the scene.
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Currency/Currency Config", fileName = "CurrencyConfig")]
    public class CurrencyConfig : ScriptableObject
    {
        [SerializeField] private List<CurrencyDefinition> currencies = new List<CurrencyDefinition>();

        public IReadOnlyList<CurrencyDefinition> Currencies => currencies;

        public CurrencyDefinition Find(string currencyId)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                return null;
            }

            for (int i = 0; i < currencies.Count; i++)
            {
                CurrencyDefinition def = currencies[i];
                if (def != null && def.Id == currencyId)
                {
                    return def;
                }
            }
            return null;
        }
    }
}
