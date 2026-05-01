using System;
using System.Collections;
using AnadoluCiftligi.Animals;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Products;
using UnityEngine;
using UnityEngine.Pool;

namespace AnadoluCiftligi.Drops
{
    /// <summary>
    /// Scene-bound singleton that listens to <see cref="AnimalProductHarvestedEvent"/>
    /// and spawns <see cref="ProductDrop"/> instances flying from the harvested
    /// animal toward a configured target. Backed by <see cref="ObjectPool{T}"/>
    /// to avoid Instantiate/Destroy GC cost during gameplay; drops are SetActive
    /// toggled rather than recreated.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProductDropSpawner : MonoBehaviour
    {
        private static ProductDropSpawner instance;

        public static ProductDropSpawner Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[ProductDropSpawner] Instance accessed but no ProductDropSpawner exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("References")]
        [SerializeField] private ProductDrop dropPrefab;

        [Tooltip("World-space transform that drops fly toward. Place an empty GameObject near the gold UI in world coordinates.")]
        [SerializeField] private Transform target;

        [Tooltip("Optional parent transform for pooled drops. If null, drops are parented to this spawner GameObject.")]
        [SerializeField] private Transform dropsRoot;

        [Header("Spawn Behavior")]
        [Tooltip("Maximum drops spawned per harvest. If amount exceeds this, the last drop carries the remainder.")]
        [SerializeField, Min(1)] private int maxDropsPerHarvest = 10;

        [Tooltip("Random radius around the harvest origin used to fan out drops so they don't perfectly stack.")]
        [SerializeField, Min(0f)] private float fanOutRadius = 0.4f;

        [Tooltip("Delay between consecutive drops in a single harvest. Set to 0 for simultaneous spawn.")]
        [SerializeField, Min(0f)] private float perDropDelay = 0.05f;

        [Header("Pool")]
        [Tooltip("Initial Stack capacity for the drop pool. Items are still created lazily on demand.")]
        [SerializeField, Min(1)] private int defaultCapacity = 16;

        [Tooltip("Maximum number of drops kept in the pool. Excess returns are destroyed instead of pooled.")]
        [SerializeField, Min(1)] private int maxSize = 64;

        private ObjectPool<ProductDrop> pool;
        private Action<ProductDrop> cachedReleaser;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[ProductDropSpawner] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (dropPrefab == null)
            {
                Debug.LogError("[ProductDropSpawner] dropPrefab reference is missing. Drops will not spawn.");
            }
            if (target == null)
            {
                Debug.LogWarning("[ProductDropSpawner] target reference is missing. Drops will fly toward world origin (0,0,0).");
            }

            if (dropsRoot == null)
            {
                dropsRoot = transform;
            }

            cachedReleaser = ReleaseDropToPool;

            pool = new ObjectPool<ProductDrop>(
                createFunc: CreatePooledDrop,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPooled,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);

            EventBus.Subscribe<AnimalProductHarvestedEvent>(OnHarvested);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AnimalProductHarvestedEvent>(OnHarvested);
            pool?.Clear();
            if (instance == this)
            {
                instance = null;
            }
        }

        private ProductDrop CreatePooledDrop()
        {
            ProductDrop drop = Instantiate(dropPrefab, dropsRoot);
            drop.SetPoolReleaser(cachedReleaser);
            drop.gameObject.SetActive(false);
            return drop;
        }

        private void OnGetFromPool(ProductDrop drop)
        {
            if (drop != null)
            {
                drop.gameObject.SetActive(true);
            }
        }

        private void OnReturnToPool(ProductDrop drop)
        {
            if (drop != null)
            {
                drop.gameObject.SetActive(false);
            }
        }

        private void OnDestroyPooled(ProductDrop drop)
        {
            if (drop != null)
            {
                Destroy(drop.gameObject);
            }
        }

        private void ReleaseDropToPool(ProductDrop drop)
        {
            if (drop == null || pool == null)
            {
                return;
            }
            pool.Release(drop);
        }

        private void OnHarvested(AnimalProductHarvestedEvent e)
        {
            if (e.Source == null || e.Product == null || e.Amount <= 0 || dropPrefab == null || pool == null)
            {
                return;
            }

            Vector3 origin = e.Source.transform.position;
            Vector3 destination = target != null ? target.position : Vector3.zero;

            int spawnCount = Mathf.Min(e.Amount, maxDropsPerHarvest);
            int remainder = e.Amount - (spawnCount - 1);

            if (perDropDelay <= 0f || spawnCount == 1)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    int payload = (i == spawnCount - 1) ? remainder : 1;
                    SpawnOne(origin, destination, e.Product, payload);
                }
            }
            else
            {
                StartCoroutine(SpawnStaggered(origin, destination, e.Product, spawnCount, remainder));
            }
        }

        private IEnumerator SpawnStaggered(Vector3 origin, Vector3 destination, ProductDefinition product, int spawnCount, int remainder)
        {
            WaitForSeconds wait = new WaitForSeconds(perDropDelay);
            for (int i = 0; i < spawnCount; i++)
            {
                int payload = (i == spawnCount - 1) ? remainder : 1;
                SpawnOne(origin, destination, product, payload);
                if (i < spawnCount - 1)
                {
                    yield return wait;
                }
            }
        }

        private void SpawnOne(Vector3 origin, Vector3 destination, ProductDefinition product, int payloadAmount)
        {
            ProductDrop drop = pool.Get();
            if (drop == null)
            {
                return;
            }

            Vector2 fan = UnityEngine.Random.insideUnitCircle * fanOutRadius;
            Vector3 spawnPos = origin + new Vector3(fan.x, fan.y, 0f);
            drop.Launch(spawnPos, destination, product, payloadAmount);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug / Pool Stats")]
        private void EditorPoolStats()
        {
            if (pool == null)
            {
                Debug.Log("[ProductDropSpawner] Pool not initialized.");
                return;
            }
            Debug.Log($"[ProductDropSpawner] Pool active={pool.CountActive}, inactive={pool.CountInactive}, total={pool.CountAll}");
        }
#endif
    }
}
