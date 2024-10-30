using System;

namespace Pancake
{
    public class MessageBinding<T> : IEventBindingInternal<T> where T : struct, IMessage
    {
        public int InternalIndex { get; set; } = -1;

        public bool Registered => InternalIndex != -1;

        public bool Listen { get => _listen; set => SetListen(value); }
        private bool _listen;

        private Action<T> _onEvent;
        private Action _onEventNoArgs;

        Action<T> IEventBindingInternal<T>.OnEvent { get => _onEvent; set => _onEvent = value; }
        Action IEventBindingInternal<T>.OnEventArgs { get => _onEventNoArgs; set => _onEventNoArgs = value; }

        public MessageBinding(Action<T> onEvent)
        {
            _onEvent = onEvent;
            Listen = true;
        }

        public MessageBinding(Action onEventNoArgs)
        {
            _onEventNoArgs = onEventNoArgs;
            Listen = true;
        }

        public void Add(Action<T> onEvent) => _onEvent += onEvent;
        public void Remove(Action<T> onEvent) => _onEvent -= onEvent;

        public void Add(Action onEvent) => _onEventNoArgs += onEvent;
        public void Remove(Action onEvent) => _onEventNoArgs -= onEvent;

        private void SetListen(bool value)
        {
            if (value == _listen) return;

            if (value) Messenger<T>.Register(this);
            else Messenger<T>.Unregister(this);

            _listen = value;
        }


        public static implicit operator MessageBinding<T>(Action onEventNoArgs) { return new MessageBinding<T>(onEventNoArgs); }

        public static implicit operator MessageBinding<T>(Action<T> onEvent) { return new MessageBinding<T>(onEvent); }

        public static implicit operator bool(MessageBinding<T> bind) { return bind != null; }
    }
}