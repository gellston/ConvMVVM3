using System;
using System.Collections.Generic;

namespace ConvMVVM3.WPF
{
    /// <summary>
    /// Manages event subscriptions using weak references to prevent memory leaks.
    /// Automatically cleans up dead references.
    /// </summary>
    public class WeakEventManager
    {
        private readonly List<WeakReference> _listeners = new List<WeakReference>();

        /// <summary>
        /// Adds a listener using a weak reference.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(object listener)
        {
            if (listener == null) return;

            _listeners.Add(new WeakReference(listener));
            CleanupDeadReferences();
        }

        /// <summary>
        /// Raises an event to all alive listeners.
        /// </summary>
        /// <param name="action">The action to perform on each listener.</param>
        public void RaiseEvent(Action<object> action)
        {
            CleanupDeadReferences();

            foreach (var weakRef in _listeners.ToArray())
            {
                if (weakRef.Target is object target)
                {
                    action(target);
                }
            }
        }

        /// <summary>
        /// Removes dead references from the listener list.
        /// </summary>
        private void CleanupDeadReferences()
        {
            _listeners.RemoveAll(wr => !wr.IsAlive);
        }

        /// <summary>
        /// Clears all listeners.
        /// </summary>
        public void Clear()
        {
            _listeners.Clear();
        }
    }
}