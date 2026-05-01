using AnadoluCiftligi.Core;
using TMPro;
using UnityEngine;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Debug HUD label that mirrors the current <see cref="GameState"/>.
    /// Updates only on state transitions via <see cref="GameStateChangedEvent"/>;
    /// no per-frame polling. Placeholder until the currency view replaces it in FAZ 1.6.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GameStateLabel : UIView
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private string format = "State: {0}";

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

            Bind<GameStateChangedEvent>(OnStateChanged);

            // Reflect current state immediately, in case any transitions
            // already fired before this view was enabled.
            if (GameManager.HasInstance)
            {
                Render(GameManager.Instance.CurrentState);
            }
        }

        private void OnStateChanged(GameStateChangedEvent e)
        {
            Render(e.Current);
        }

        private void Render(GameState state)
        {
            if (label == null)
            {
                return;
            }
            label.text = string.Format(format, state);
        }
    }
}
