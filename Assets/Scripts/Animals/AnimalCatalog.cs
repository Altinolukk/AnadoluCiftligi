using System.Collections.Generic;
using UnityEngine;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// Aggregates every <see cref="AnimalData"/> the game knows about for
    /// id-based lookup. Required by <see cref="AnimalSpawner"/> when restoring
    /// saved animals back into AnimalData references.
    /// </summary>
    [CreateAssetMenu(menuName = "AnadoluCiftligi/Animals/Animal Catalog", fileName = "AnimalCatalog")]
    public class AnimalCatalog : ScriptableObject
    {
        [SerializeField] private List<AnimalData> entries = new List<AnimalData>();

        public IReadOnlyList<AnimalData> Entries => entries;

        public AnimalData Find(string animalId)
        {
            if (string.IsNullOrEmpty(animalId))
            {
                return null;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                AnimalData entry = entries[i];
                if (entry != null && entry.Id == animalId)
                {
                    return entry;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            HashSet<string> seen = new HashSet<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                AnimalData entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }
                if (!seen.Add(entry.Id))
                {
                    Debug.LogWarning($"[AnimalCatalog] '{name}' contains duplicate animal id '{entry.Id}'. Find() will resolve the first occurrence only.");
                }
            }
        }
#endif
    }
}
