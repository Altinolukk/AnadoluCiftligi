using System;
using System.Collections.Generic;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Save;
using UnityEngine;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// Scene-bound singleton that owns the lifecycle of all <see cref="Animal"/>
    /// instances: spawn, despawn, and persistence. Uses a single Animal prefab
    /// and configures each instance via <see cref="Animal.Initialize"/> from an
    /// <see cref="AnimalData"/> looked up in <see cref="AnimalCatalog"/>.
    /// Implements <see cref="ISavable"/> with key 'animals'.
    /// </summary>
    [DisallowMultipleComponent]
    public class AnimalSpawner : MonoBehaviour, ISavable
    {
        public const string SaveKeyValue = "animals";

        private static AnimalSpawner instance;

        public static AnimalSpawner Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[AnimalSpawner] Instance accessed but no AnimalSpawner exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("References")]
        [SerializeField] private AnimalCatalog catalog;
        [SerializeField] private Animal animalPrefab;
        [Tooltip("Optional parent transform for spawned animals. If null, animals are spawned at scene root.")]
        [SerializeField] private Transform animalsRoot;

        [Header("Spawn Area")]
        [Tooltip("Center of the rectangular area used when no explicit spawn position is provided.")]
        [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
        [Tooltip("Size (width, height) of the spawn area centered on spawnAreaCenter.")]
        [SerializeField] private Vector2 spawnAreaSize = new Vector2(8f, 5f);

        [Header("Bootstrap")]
        [Tooltip("If true, a starter animal is auto-spawned whenever load completes with zero active animals (new game or missing save partition).")]
        [SerializeField] private bool autoSpawnStarter = true;
        [Tooltip("AnimalData used for the starter spawn. Typically Chicken_T1.")]
        [SerializeField] private AnimalData starterAnimal;

        private readonly List<Animal> activeAnimals = new List<Animal>();

        public string SaveKey => SaveKeyValue;
        public IReadOnlyList<Animal> ActiveAnimals => activeAnimals;
        public int ActiveCount => activeAnimals.Count;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[AnimalSpawner] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (catalog == null)
            {
                Debug.LogError("[AnimalSpawner] AnimalCatalog reference is missing. Save restore will fail.");
            }
            if (animalPrefab == null)
            {
                Debug.LogError("[AnimalSpawner] Animal prefab reference is missing. Spawn calls will be ignored.");
            }

            SaveSystem.Register(this);
            EventBus.Subscribe<LoadCompletedEvent>(OnLoadCompleted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<LoadCompletedEvent>(OnLoadCompleted);
            SaveSystem.Unregister(this);
            if (instance == this)
            {
                instance = null;
            }
        }

        public Animal Spawn(AnimalData data)
        {
            return Spawn(data, RandomPosition());
        }

        public Animal Spawn(AnimalData data, Vector2 position)
        {
            if (data == null)
            {
                Debug.LogWarning("[AnimalSpawner] Spawn called with null AnimalData.");
                return null;
            }
            if (animalPrefab == null)
            {
                Debug.LogError("[AnimalSpawner] Cannot spawn: animal prefab reference is missing.");
                return null;
            }

            Vector3 worldPosition = new Vector3(position.x, position.y, 0f);
            Animal animal = Instantiate(animalPrefab, worldPosition, Quaternion.identity, animalsRoot);
            animal.name = data.Id;
            animal.Initialize(data);

            activeAnimals.Add(animal);
            EventBus.Publish(new AnimalSpawnedEvent(animal));
            return animal;
        }

        public void Despawn(Animal animal)
        {
            if (animal == null)
            {
                return;
            }

            bool removed = activeAnimals.Remove(animal);
            if (removed)
            {
                EventBus.Publish(new AnimalDespawnedEvent(animal));
            }

            if (animal != null)
            {
                Destroy(animal.gameObject);
            }
        }

        public void DespawnAll()
        {
            for (int i = activeAnimals.Count - 1; i >= 0; i--)
            {
                Animal animal = activeAnimals[i];
                if (animal != null)
                {
                    EventBus.Publish(new AnimalDespawnedEvent(animal));
                    Destroy(animal.gameObject);
                }
            }
            activeAnimals.Clear();
        }

        public Vector2 RandomPosition()
        {
            Vector2 half = spawnAreaSize * 0.5f;
            float x = UnityEngine.Random.Range(-half.x, half.x);
            float y = UnityEngine.Random.Range(-half.y, half.y);
            return spawnAreaCenter + new Vector2(x, y);
        }

        private void OnLoadCompleted(LoadCompletedEvent e)
        {
            if (!autoSpawnStarter)
            {
                return;
            }

            if (activeAnimals.Count > 0)
            {
                return;
            }

            if (starterAnimal == null)
            {
                Debug.LogWarning("[AnimalSpawner] autoSpawnStarter is enabled but starterAnimal is not assigned. Skipping.");
                return;
            }

            Animal spawned = Spawn(starterAnimal, RandomPosition());
            if (spawned != null)
            {
                Debug.Log($"[AnimalSpawner] Starter '{starterAnimal.Id}' auto-spawned (active animals were empty).");
            }
        }

        public string CaptureState()
        {
            AnimalSpawnerSaveData payload = new AnimalSpawnerSaveData();
            for (int i = 0; i < activeAnimals.Count; i++)
            {
                Animal animal = activeAnimals[i];
                if (animal == null)
                {
                    continue;
                }
                payload.Animals.Add(animal.CaptureSaveData());
            }
            return JsonUtility.ToJson(payload);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            if (catalog == null || animalPrefab == null)
            {
                Debug.LogError("[AnimalSpawner] Cannot restore: catalog or prefab reference is missing.");
                return;
            }

            AnimalSpawnerSaveData payload = JsonUtility.FromJson<AnimalSpawnerSaveData>(json);
            if (payload == null || payload.Animals == null)
            {
                return;
            }

            DespawnAll();

            for (int i = 0; i < payload.Animals.Count; i++)
            {
                AnimalSaveData entry = payload.Animals[i];
                if (entry == null || string.IsNullOrEmpty(entry.AnimalId))
                {
                    continue;
                }

                AnimalData data = catalog.Find(entry.AnimalId);
                if (data == null)
                {
                    Debug.LogWarning($"[AnimalSpawner] Save references unknown animal id '{entry.AnimalId}'. Skipping.");
                    continue;
                }

                Animal spawned = Spawn(data, new Vector2(entry.PosX, entry.PosY));
                if (spawned != null)
                {
                    spawned.RestoreSaveData(entry);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug / Spawn Random From Catalog")]
        private void EditorSpawnRandom()
        {
            if (catalog == null || catalog.Entries.Count == 0)
            {
                Debug.LogWarning("[AnimalSpawner] No catalog entries to spawn from.");
                return;
            }
            int index = UnityEngine.Random.Range(0, catalog.Entries.Count);
            AnimalData pick = catalog.Entries[index];
            if (pick != null)
            {
                Spawn(pick, RandomPosition());
            }
        }

        [ContextMenu("Debug / Despawn All")]
        private void EditorDespawnAll()
        {
            DespawnAll();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.7f, 1f, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
            Gizmos.color = new Color(0.1f, 0.7f, 1f, 1f);
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
#endif

        [Serializable]
        private class AnimalSpawnerSaveData
        {
            public List<AnimalSaveData> Animals = new List<AnimalSaveData>();
        }
    }
}
