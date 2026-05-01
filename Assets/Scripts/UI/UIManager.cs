using UnityEngine;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Top-level UI coordinator. Holds the root canvas reference and serves
    /// as the designated parent for high-level panel switching introduced in
    /// later phases. Intentionally NOT a singleton; access through inspector
    /// references where needed. Multiple instances per scene are not supported.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas rootCanvas;

        public Canvas RootCanvas => rootCanvas;

        private void Awake()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInChildren<Canvas>();
            }

            if (rootCanvas == null)
            {
                Debug.LogWarning("[UIManager] No Canvas reference assigned and none found in children. Assign rootCanvas in the inspector.");
            }
        }
    }
}
