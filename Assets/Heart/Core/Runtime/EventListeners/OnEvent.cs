using Sisus.Init;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;

namespace Pancake
{
    /// <summary>
    /// Base class for components that invoke an <see cref="UnityEvent"/> in reaction
    /// to an <typeparamref name="TEvent"/> occurring.
    /// </summary>
    [RequireDerived]
    [EditorIcon("icon_event_listener")]
    public abstract class OnEvent<TEvent> : MonoBehaviour<TEvent> where TEvent : IEvent
    {
        [SerializeField] private UnityEvent reaction = new();
        private TEvent _event;

        protected override void Init(TEvent @event) => _event = @event;
        private void OnEnable() => _event.AddListener(reaction.Invoke);
        private void OnDisable() => _event.RemoveListener(reaction.Invoke);
    }
}