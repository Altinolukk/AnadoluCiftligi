using AnadoluCiftligi.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// Runtime instance of a producer entity. Drives the production timer
    /// against its <see cref="AnimalData"/>, accumulates pending products up
    /// to a cap, and emits <see cref="AnimalProductHarvestedEvent"/> when the
    /// player taps the sprite to collect.
    /// Requires a <see cref="Collider2D"/> on this GameObject (or a child) and
    /// a <c>Physics2DRaycaster</c> on the active camera for click detection.
    /// </summary>
    [DisallowMultipleComponent]
    public class Animal : MonoBehaviour, IPointerClickHandler
    {
        [Header("Data")]
        [SerializeField] private AnimalData data;

        [Header("Production")]
        [Tooltip("Maximum number of pending products that can accumulate. Production pauses at this cap until the player harvests.")]
        [SerializeField, Min(1)] private int maxPendingProducts = 5;

        [Header("Visual")]
        [Tooltip("Optional. If left empty the SpriteRenderer is auto-discovered on this GameObject or its children.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public AnimalData Data => data;
        public int PendingProducts => pendingProducts;
        public int MaxPendingProducts => maxPendingProducts;
        public bool HasPending => pendingProducts > 0;
        public bool IsAtCap => pendingProducts >= maxPendingProducts;

        private int pendingProducts;
        private float productionTimer;
        private bool initialized;

        /// <summary>
        /// Assigns AnimalData at runtime (used by AnimalSpawner / merge flow).
        /// Calling this resets the production timer and pending count.
        /// </summary>
        public void Initialize(AnimalData newData)
        {
            data = newData;
            pendingProducts = 0;
            productionTimer = 0f;
            ApplyData();
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        private void Start()
        {
            ApplyData();
        }

        private void ApplyData()
        {
            if (data == null)
            {
                Debug.LogWarning($"[Animal] '{name}' has no AnimalData assigned.", this);
                initialized = false;
                return;
            }

            if (spriteRenderer != null && data.Sprite != null)
            {
                spriteRenderer.sprite = data.Sprite;
            }

            initialized = true;
        }

        private void Update()
        {
            if (!initialized || data == null)
            {
                return;
            }

            if (pendingProducts >= maxPendingProducts)
            {
                return;
            }

            productionTimer += Time.deltaTime;
            float interval = data.ProductionInterval;
            if (interval <= 0f)
            {
                return;
            }

            while (productionTimer >= interval && pendingProducts < maxPendingProducts)
            {
                productionTimer -= interval;
                int next = pendingProducts + data.ProductionAmount;
                if (next > maxPendingProducts)
                {
                    next = maxPendingProducts;
                }
                pendingProducts = next;
                EventBus.Publish(new AnimalPendingChangedEvent(this, pendingProducts, maxPendingProducts));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Harvest();
        }

        public void Harvest()
        {
            if (!initialized || data == null || pendingProducts <= 0)
            {
                return;
            }

            if (data.Product == null)
            {
                Debug.LogWarning($"[Animal] '{data.Id}' has no Product assigned. Harvest skipped.", this);
                return;
            }

            int collected = pendingProducts;
            pendingProducts = 0;

            EventBus.Publish(new AnimalPendingChangedEvent(this, 0, maxPendingProducts));
            EventBus.Publish(new AnimalProductHarvestedEvent(this, data, data.Product, collected));
        }

#if UNITY_EDITOR
        [ContextMenu("Debug / Force Produce One Cycle")]
        private void EditorForceProduce()
        {
            if (data == null)
            {
                return;
            }
            int next = Mathf.Min(pendingProducts + data.ProductionAmount, maxPendingProducts);
            pendingProducts = next;
            EventBus.Publish(new AnimalPendingChangedEvent(this, pendingProducts, maxPendingProducts));
            Debug.Log($"[Animal] '{data.Id}' force-produced. Pending: {pendingProducts}/{maxPendingProducts}");
        }

        [ContextMenu("Debug / Force Harvest")]
        private void EditorForceHarvest()
        {
            Harvest();
        }
#endif
    }
}
