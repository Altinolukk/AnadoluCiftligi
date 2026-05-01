using AnadoluCiftligi.Events;

namespace AnadoluCiftligi.Save
{
    public readonly struct SaveCompletedEvent : IGameEvent
    {
        public readonly bool Success;

        public SaveCompletedEvent(bool success)
        {
            Success = success;
        }
    }

    public readonly struct LoadCompletedEvent : IGameEvent
    {
        public readonly bool Success;
        public readonly bool IsNewGame;

        public LoadCompletedEvent(bool success, bool isNewGame)
        {
            Success = success;
            IsNewGame = isNewGame;
        }
    }
}
