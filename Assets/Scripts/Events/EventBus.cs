using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnadoluCiftligi.Events
{
    /// <summary>
    /// Type-safe global publish/subscribe hub. Subscribers register an
    /// <see cref="Action{T}"/> for a concrete event type and receive synchronous
    /// invocations on the calling thread (Unity main thread by convention).
    /// Event payloads must be readonly structs implementing <see cref="IGameEvent"/>
    /// to avoid GC allocations during publish.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            Type type = typeof(T);
            if (handlers.TryGetValue(type, out Delegate existing))
            {
                handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                handlers[type] = handler;
            }
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }

            Type type = typeof(T);
            if (!handlers.TryGetValue(type, out Delegate existing))
            {
                return;
            }

            Delegate updated = Delegate.Remove(existing, handler);
            if (updated == null)
            {
                handlers.Remove(type);
            }
            else
            {
                handlers[type] = updated;
            }
        }

        public static void Publish<T>(T payload) where T : struct, IGameEvent
        {
            if (!handlers.TryGetValue(typeof(T), out Delegate existing))
            {
                return;
            }

            Delegate[] invocations = existing.GetInvocationList();
            for (int i = 0; i < invocations.Length; i++)
            {
                try
                {
                    ((Action<T>)invocations[i]).Invoke(payload);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Clear()
        {
            handlers.Clear();
        }

        /// <summary>
        /// Resets the bus when entering Play Mode. Required when "Reload Domain"
        /// is disabled in Editor settings, otherwise stale handlers leak between sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            handlers.Clear();
        }
    }
}
