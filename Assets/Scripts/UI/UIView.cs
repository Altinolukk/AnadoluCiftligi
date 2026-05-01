using System;
using System.Collections.Generic;
using AnadoluCiftligi.Events;
using UnityEngine;

namespace AnadoluCiftligi.UI
{
    /// <summary>
    /// Base class for event-driven UI elements. Subclasses override
    /// <see cref="BindEvents"/> and use <see cref="Bind{T}"/> to subscribe;
    /// subscriptions are automatically removed in OnDisable to prevent leaks
    /// when the view is hidden, destroyed, or the scene reloads.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        private readonly List<Action> unsubscribers = new List<Action>();

        protected virtual void OnEnable()
        {
            BindEvents();
        }

        protected virtual void OnDisable()
        {
            for (int i = 0; i < unsubscribers.Count; i++)
            {
                try
                {
                    unsubscribers[i]?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            unsubscribers.Clear();
        }

        /// <summary>
        /// Override to declare event subscriptions. Use <see cref="Bind{T}"/>
        /// to register handlers that will be unsubscribed automatically.
        /// </summary>
        protected abstract void BindEvents();

        protected void Bind<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                return;
            }
            EventBus.Subscribe(handler);
            unsubscribers.Add(() => EventBus.Unsubscribe(handler));
        }
    }
}
