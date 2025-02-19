using System;

// ReSharper disable InconsistentNaming
namespace Pancake.Elm
{
    public class Sub<T>
    {
        internal event Action<T> OnWatch;
        private readonly Action<T> _cachedInvoke;

        public IEffect<T> Effect { get; private set; }

        public Sub(IEffect<T> effect)
        {
            _cachedInvoke = Invoke;
            Effect = effect;
            Effect.Add(_cachedInvoke);
        }

        private void Invoke(T val) => OnWatch?.Invoke(val);

        public Sub<U> Map<U>(Func<T, U> func)
        {
            Effect.Remove(_cachedInvoke);
            return new Sub<U>(new MappedEffect<T, U>(Effect, func));
        }

        internal void UpdateEffect(IEffect<T> effect)
        {
            Effect.Remove(_cachedInvoke);
            Effect = effect;
            Effect.Add(_cachedInvoke);
        }

        public static readonly Sub<T> None = new(new NoneEffect<T>());

        public static Sub<T> Batch(Sub<T>[] subs)
        {
            var effects = new IEffect<T>[subs.Length];
            for (var i = 0; i < subs.Length; i++)
            {
                var sub = subs[i];
                sub.Effect.Remove(sub.Invoke);
                effects[i] = sub.Effect;
            }

            return new Sub<T>(new BatchingEffect<T>(effects));
        }
    }

    public interface IEffect<out T>
    {
        void Add(Action<T> handler);
        void Remove(Action<T> handler);
    }

    internal class NoneEffect<T> : IEffect<T>
    {
        public void Add(Action<T> handler) { }

        public void Remove(Action<T> handler) { }
    }

    internal class BatchingEffect<T> : IEffect<T>
    {
        private event Action<T> OnOccurrence;

        public BatchingEffect(IEffect<T>[] effects)
        {
            foreach (var effect in effects)
            {
                effect.Add(v => OnOccurrence?.Invoke(v));
            }
        }

        public void Add(Action<T> handler) { OnOccurrence += handler; }

        public void Remove(Action<T> handler) { OnOccurrence -= handler; }
    }

    internal class MappedEffect<T, U> : IEffect<U>
    {
        private event Action<U> OnOccurrence;

        public MappedEffect(IEffect<T> effect, Func<T, U> func) { effect.Add(val => OnOccurrence?.Invoke(func(val))); }

        public void Add(Action<U> handler) { OnOccurrence += handler; }

        public void Remove(Action<U> handler) { OnOccurrence -= handler; }
    }
}