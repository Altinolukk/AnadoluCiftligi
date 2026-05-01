using System;
using System.Collections.Generic;
using System.IO;
using AnadoluCiftligi.Events;
using UnityEngine;

namespace AnadoluCiftligi.Save
{
    /// <summary>
    /// JSON-based save orchestrator. Modules implement <see cref="ISavable"/> and
    /// register themselves; SaveSystem delegates serialization to each module and
    /// writes a single partitioned save file under <see cref="Application.persistentDataPath"/>.
    /// Lifecycle wiring (when to call Save/Load) is owned by GameManager — this
    /// class deliberately stays platform/state agnostic.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "save.json";
        private const string TempSuffix = ".tmp";

        private static readonly List<ISavable> savables = new List<ISavable>();

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static bool HasSaveFile => File.Exists(SavePath);

        public static int RegisteredCount => savables.Count;

        public static void Register(ISavable savable)
        {
            if (savable == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(savable.SaveKey))
            {
                Debug.LogError($"[SaveSystem] Cannot register savable with empty SaveKey: {savable.GetType().Name}");
                return;
            }

            if (savables.Contains(savable))
            {
                return;
            }

            for (int i = 0; i < savables.Count; i++)
            {
                if (savables[i].SaveKey == savable.SaveKey)
                {
                    Debug.LogError($"[SaveSystem] Duplicate SaveKey '{savable.SaveKey}' detected. Existing: {savables[i].GetType().Name}, New: {savable.GetType().Name}. New registration ignored.");
                    return;
                }
            }

            savables.Add(savable);
        }

        public static void Unregister(ISavable savable)
        {
            if (savable == null)
            {
                return;
            }
            savables.Remove(savable);
        }

        public static bool Load()
        {
            if (!HasSaveFile)
            {
                Debug.Log("[SaveSystem] No save file found. Starting new game.");
                EventBus.Publish(new LoadCompletedEvent(true, true));
                return true;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveFile file = JsonUtility.FromJson<SaveFile>(json);
                if (file == null)
                {
                    Debug.LogError("[SaveSystem] Save file deserialized to null. Treating as new game.");
                    EventBus.Publish(new LoadCompletedEvent(false, true));
                    return false;
                }

                Dictionary<string, string> partitionMap = new Dictionary<string, string>(file.Partitions.Count);
                for (int i = 0; i < file.Partitions.Count; i++)
                {
                    SavePartition partition = file.Partitions[i];
                    if (!string.IsNullOrEmpty(partition.Key))
                    {
                        partitionMap[partition.Key] = partition.DataJson ?? string.Empty;
                    }
                }

                int restored = 0;
                for (int i = 0; i < savables.Count; i++)
                {
                    ISavable savable = savables[i];
                    if (!partitionMap.TryGetValue(savable.SaveKey, out string data))
                    {
                        continue;
                    }

                    try
                    {
                        savable.RestoreState(data);
                        restored++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                Debug.Log($"[SaveSystem] Loaded save (version {file.Version}, savedAt {file.SavedAtIso}). Restored {restored}/{savables.Count} modules.");
                EventBus.Publish(new LoadCompletedEvent(true, false));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EventBus.Publish(new LoadCompletedEvent(false, false));
                return false;
            }
        }

        public static bool Save()
        {
            try
            {
                SaveFile file = new SaveFile
                {
                    Version = SaveFile.CurrentVersion,
                    SavedAtIso = DateTime.UtcNow.ToString("o")
                };

                for (int i = 0; i < savables.Count; i++)
                {
                    ISavable savable = savables[i];
                    string data;
                    try
                    {
                        data = savable.CaptureState();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        continue;
                    }

                    file.Partitions.Add(new SavePartition
                    {
                        Key = savable.SaveKey,
                        DataJson = data ?? string.Empty
                    });
                }

                string json = JsonUtility.ToJson(file, true);
                string targetPath = SavePath;
                string tempPath = targetPath + TempSuffix;

                File.WriteAllText(tempPath, json);
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Move(tempPath, targetPath);

                Debug.Log($"[SaveSystem] Saved {file.Partitions.Count} partitions to {targetPath}");
                EventBus.Publish(new SaveCompletedEvent(true));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EventBus.Publish(new SaveCompletedEvent(false));
                return false;
            }
        }

        public static void DeleteSaveFile()
        {
            if (!HasSaveFile)
            {
                return;
            }

            try
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            savables.Clear();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("AnadoluCiftligi/Save/Delete Save File")]
        private static void EditorDeleteSaveFile()
        {
            DeleteSaveFile();
            UnityEditor.EditorUtility.DisplayDialog(
                "AnadoluCiftligi",
                HasSaveFile ? "Save file still exists (deletion failed)." : "Save file deleted.",
                "OK");
        }

        [UnityEditor.MenuItem("AnadoluCiftligi/Save/Reveal Save Folder")]
        private static void EditorRevealSaveFolder()
        {
            UnityEditor.EditorUtility.RevealInFinder(SavePath);
        }
#endif
    }
}
