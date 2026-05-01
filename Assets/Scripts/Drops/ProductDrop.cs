using System;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;
using UnityEngine;

namespace AnadoluCiftligi.Drops
{
    /// <summary>
    /// Animated 2D sprite that flies from a source position to a target along
    /// a sine-shaped arc. On arrival publishes <see cref="ProductDropCollectedEvent"/>
    /// and either returns to the spawner's object pool (when a releaser is bound)
    /// or self-destructs as a fallback.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProductDrop : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Animation")]
        [Tooltip("Total flight time from source to target, in seconds.")]
        [SerializeField, Min(0.05f)] private float duration = 0.6f;

        [Tooltip("Peak vertical offset of the arc above the linear path. Set to 0 for straight-line flight.")]
        [SerializeField, Min(0f)] private float arcHeight = 1.5f;

        [Tooltip("Scale curve over normalized travel time (x: 0..1, y: scale multiplier).")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.7f);

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float elapsed;
        private bool active;
        private ProductDefinition product;
        private int amount;
        private Action<ProductDrop> releaseToPool;

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

        /// <summary>
        /// Bound once by <see cref="ProductDropSpawner"/> at pool createFunc time.
        /// On arrival, this callback returns the drop to the pool; if null, the
        /// drop self-destructs (legacy / non-pooled path).
        /// </summary>
        public void SetPoolReleaser(Action<ProductDrop> releaser)
        {
            releaseToPool = releaser;
        }

        public void Launch(Vector3 from, Vector3 to, ProductDefinition payloadProduct, int payloadAmount)
        {
            product = payloadProduct;
            amount = payloadAmount > 0 ? payloadAmount : 0;

            startPosition = from;
            targetPosition = to;
            elapsed = 0f;
            active = true;

            transform.position = from;
            transform.localScale = Vector3.one;

            if (spriteRenderer != null && product != null && product.Sprite != null)
            {
                spriteRenderer.sprite = product.Sprite;
            }
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (t >= 1f)
            {
                Arrive();
                return;
            }

            Vector3 lerped = Vector3.Lerp(startPosition, targetPosition, t);
            lerped.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = lerped;

            float scale = scaleCurve.Evaluate(t);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void Arrive()
        {
            active = false;
            transform.position = targetPosition;
            EventBus.Publish(new ProductDropCollectedEvent(product, amount));

            if (releaseToPool != null)
            {
                releaseToPool(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
