namespace AnadoluCiftligi.Events
{
    /// <summary>
    /// Marker interface for events published through <see cref="EventBus"/>.
    /// Implementations should be readonly structs to keep publish allocation-free.
    /// </summary>
    public interface IGameEvent
    {
    }
}
