using System;

namespace AnadoluCiftligi.Animals
{
    /// <summary>
    /// JsonUtility-compatible snapshot of a single <see cref="Animal"/>'s
    /// runtime state. Captured by Animal.CaptureSaveData(), restored via
    /// Animal.RestoreSaveData(). The animal id is resolved through
    /// <see cref="AnimalCatalog"/> on load.
    /// </summary>
    [Serializable]
    public class AnimalSaveData
    {
        public string AnimalId = string.Empty;
        public float PosX;
        public float PosY;
        public int PendingProducts;
        public float ProductionTimer;
    }
}
