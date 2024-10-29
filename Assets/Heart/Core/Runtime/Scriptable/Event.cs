using UnityEngine;
using UnityEngine.Events;

namespace Pancake
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> asset that represents an event.
    /// <para>
    /// Whenever the event is <see cref="Trigger">triggered</see> all
    /// methods that have subscribed to receive a callback are executed.
    /// </para>
    /// </summary>
    public abstract class Event : ScriptableObject, IEvent, IEventTrigger
    {
        protected const string CREATE_ASSET_MENU = "Pancake/Scriptable/Events/";

        private event UnityAction Listeners;

#if DEBUG
        [SerializeField] private bool debugTrigger;
#endif

        /// <inheritdoc/>
        public void AddListener(UnityAction method) { Listeners += method; }

        /// <inheritdoc/>
        public void RemoveListener(UnityAction method) { Listeners -= method; }

        /// <inheritdoc/>
        public void Trigger()
        {
#if DEBUG
            if (debugTrigger) Debug.Log(GenerateOnTriggeredDebugMessage(), this);
#endif

            Listeners?.Invoke();
        }

#if DEBUG
        private string GenerateOnTriggeredDebugMessage()
        {
            if (Listeners is null) return name + " Triggered with no listeners.";

            var invocationList = Listeners.GetInvocationList();
            var sb = new System.Text.StringBuilder();
            sb.Append(name);
            sb.Append(" Triggered with ");
            sb.Append(invocationList.Length);
            sb.Append(" listeners.");

            foreach (var invocation in invocationList)
            {
                sb.AppendLine();

                if (invocation.Method.DeclaringType != null)
                {
                    sb.Append(invocation.Method.DeclaringType.Name);
                    sb.Append(".");
                }

                sb.Append(invocation.Method.Name);
            }

            return sb.ToString();
        }
#endif
    }
}