using System;
using System.Collections.Generic;

namespace AnadoluCiftligi.Save
{
    /// <summary>
    /// Top-level JsonUtility-compatible save container. Each registered
    /// <see cref="ISavable"/> contributes one <see cref="SavePartition"/> entry.
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public string SavedAtIso = string.Empty;
        public List<SavePartition> Partitions = new List<SavePartition>();
    }

    [Serializable]
    public class SavePartition
    {
        public string Key = string.Empty;
        public string DataJson = string.Empty;
    }
}
