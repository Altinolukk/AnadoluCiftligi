using AnadoluCiftligi.Events;

namespace AnadoluCiftligi.Core
{
    public readonly struct GameStateChangedEvent : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;

        public GameStateChangedEvent(GameState previous, GameState current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct ApplicationPausedEvent : IGameEvent
    {
    }

    public readonly struct ApplicationResumedEvent : IGameEvent
    {
    }
}
