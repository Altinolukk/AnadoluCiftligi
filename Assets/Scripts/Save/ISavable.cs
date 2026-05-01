namespace AnadoluCiftligi.Save
{
    /// <summary>
    /// Implemented by any system that wants its state persisted across sessions.
    /// Each implementor owns its own JSON serialization format; the SaveSystem only
    /// orchestrates partitioning by <see cref="SaveKey"/>.
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Stable, unique identifier used as the partition key inside the save file.
        /// Changing this for an existing module breaks save compatibility — bump
        /// the module's internal version field and migrate inside RestoreState instead.
        /// </summary>
        string SaveKey { get; }

        /// <summary>
        /// Serialize the current state to a JSON string. Return an empty string
        /// or "{}" if there is nothing to persist this frame.
        /// </summary>
        string CaptureState();

        /// <summary>
        /// Apply the previously captured state. Called only when a partition
        /// for <see cref="SaveKey"/> exists in the save file. Implementors must
        /// be defensive about malformed or older-version JSON.
        /// </summary>
        void RestoreState(string json);
    }
}
