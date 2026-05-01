using UnityEngine;
using AnadoluCiftligi.Events;
using AnadoluCiftligi.Save;

namespace AnadoluCiftligi.Core
{
    /// <summary>
    /// Application-wide lifecycle owner. Single source of truth for game state.
    /// Must be placed as an explicit GameObject in the bootstrap scene; never auto-created.
    /// State changes and application pause/resume events are broadcast via <see cref="EventBus"/>.
    /// Owns the high-level save/load triggering: Load during Loading state, Save on
    /// application pause when actively playing.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[GameManager] Instance accessed but no GameManager exists in the active scene.");
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        [Header("Bootstrap")]
        [Tooltip("If enabled, GameManager auto-progresses Boot -> Loading -> Ready -> Playing on Start. Disable to drive states manually (e.g., from a splash scene).")]
        [SerializeField] private bool autoStartOnAwake = true;

        [Tooltip("If enabled, SaveSystem.Save() is invoked when the application pauses while in Playing state.")]
        [SerializeField] private bool autoSaveOnPause = true;

        public GameState CurrentState { get; private set; } = GameState.Boot;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[GameManager] Duplicate instance detected. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (transform.parent != null)
            {
                Debug.LogWarning("[GameManager] GameManager should be a root GameObject for DontDestroyOnLoad to work correctly.");
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (!autoStartOnAwake)
            {
                return;
            }

            ChangeState(GameState.Loading);
            SaveSystem.Load();
            ChangeState(GameState.Ready);
            ChangeState(GameState.Playing);
        }

        public void ChangeState(GameState next)
        {
            if (next == CurrentState)
            {
                return;
            }

            GameState previous = CurrentState;
            CurrentState = next;
            Debug.Log($"[GameManager] State {previous} -> {next}");
            EventBus.Publish(new GameStateChangedEvent(previous, next));
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                bool wasPlaying = CurrentState == GameState.Playing;
                if (wasPlaying)
                {
                    ChangeState(GameState.Paused);
                }
                EventBus.Publish(new ApplicationPausedEvent());

                if (wasPlaying && autoSaveOnPause)
                {
                    SaveSystem.Save();
                }
            }
            else
            {
                if (CurrentState == GameState.Paused)
                {
                    ChangeState(GameState.Playing);
                }
                EventBus.Publish(new ApplicationResumedEvent());
                // FAZ 6.1: OfflineProgressCalculator subscribes here.
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Android primarily uses OnApplicationPause; this kept for editor/desktop parity.
            if (!hasFocus && CurrentState == GameState.Playing)
            {
                EventBus.Publish(new ApplicationPausedEvent());
            }
            else if (hasFocus && CurrentState == GameState.Paused)
            {
                EventBus.Publish(new ApplicationResumedEvent());
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
