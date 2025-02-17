using System;

// ReSharper disable InconsistentNaming
namespace Pancake.Elm
{
    public class Sub<T>
    {
        public event Action<T> OnWatch;

        private readonly IEffect<T> _effect;

        public Sub(IEffect<T> effect)
        {
            _effect = effect;
            _effect.OnOccurrence += Invoke;
        }

        private void Invoke(T val) => OnWatch?.Invoke(val);

        public Sub<U> Map<U>(Func<T, U> func)
        {
            _effect.OnOccurrence -= Invoke;
            return new Sub<U>(new MappedEffect<T, U>(_effect, func));
        }

        public static Sub<T> None => new(new NoneEffect<T>());

        public static Sub<T> Batch(Sub<T>[] subs)
        {
            var effects = new IEffect<T>[subs.Length];
            for (var i = 0; i < subs.Length; i++)
            {
                var sub = subs[i];
                sub._effect.OnOccurrence -= sub.Invoke;
                effects[i] = sub._effect;
            }

            return new Sub<T>(new BatchingEffect<T>(effects));
        }
    }

    public interface IEffect<T>
    {
        event Action<T> OnOccurrence;
    }

    internal class NoneEffect<T> : IEffect<T>
    {
        public event Action<T> OnOccurrence;
    }

    internal class BatchingEffect<T> : IEffect<T>
    {
        public event Action<T> OnOccurrence;

        public BatchingEffect(IEffect<T>[] effects)
        {
            foreach (var effect in effects)
            {
                effect.OnOccurrence += v => OnOccurrence?.Invoke(v);
            }
        }
    }

    internal class MappedEffect<T, U> : IEffect<U>
    {
        public event Action<U> OnOccurrence;

        public MappedEffect(IEffect<T> effect, Func<T, U> func) { effect.OnOccurrence += val => OnOccurrence?.Invoke(func(val)); }
    }
}