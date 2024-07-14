using UnityEngine.Events;

namespace Pancake
{
    /// <summary>
    /// Represents an event that can be listened to.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Subscribe to receive a callback whenever the event occurs.
        /// </summary>
        /// <param name="method"> Method to invoke whenever the event occurs. </param>
        void AddListener(UnityAction method);

        /// <summary>
        /// Unsubscribe from receiving a callback whenever the event occurs.
        /// </summary>
        /// <param name="method"> Method to no longer invoke whenever the event occurs. </param>
        void RemoveListener(UnityAction method);
    }
}