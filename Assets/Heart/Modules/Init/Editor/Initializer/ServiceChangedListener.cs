using System;
using System.Diagnostics.CodeAnalysis;

namespace Sisus.Init.EditorOnly.Internal
{
    internal abstract class ServiceChangedListener : IDisposable
    {
        public static ServiceChangedListener Create([DisallowNull] Type argumentType, [DisallowNull] Action onChangedCallback)
            => (ServiceChangedListener)typeof(ServiceChangedListener<>).MakeGenericType(argumentType).GetConstructor(new[] { typeof(Action) }).Invoke(new object[] { onChangedCallback });

        public static void UpdateAll(ref ServiceChangedListener[] listeners, Type[] serviceTypes, Action onAnyServiceChanged)
        {
            int count = serviceTypes.Length;
            if(listeners.Length != count)
            {
                for(int i = listeners.Length - 1; i >= count; i--)
                {
                    listeners[i].Dispose();
                }

                Array.Resize(ref listeners, count);
            }

            for(int i = 0; i < count; i++)
            {
                listeners[i] ??= Create(serviceTypes[i], onAnyServiceChanged);
            }
        }

        public static void DisposeAll(ref ServiceChangedListener[] listeners)
        {
            foreach(var listener in listeners)
            {
                listener?.Dispose();
            }

            Array.Resize(ref listeners, 0);
        }

        public abstract void Dispose();
    }

    internal sealed class ServiceChangedListener<TService> : ServiceChangedListener
    {
        private readonly Action onChangedCallback;

        public ServiceChangedListener(Action onChangedCallback)
        {
            Service.AddInstanceChangedListener<TService>(OnServiceChanged);
            this.onChangedCallback = onChangedCallback;
        }

        public override void Dispose() => Service.RemoveInstanceChangedListener<TService>(OnServiceChanged);
        public void OnServiceChanged(Clients clients, [AllowNull] TService oldInstance, [AllowNull] TService newInstance) => onChangedCallback();
    }
}