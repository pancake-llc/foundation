using System;

namespace Pancake.Sound
{
    public interface IRecyclable<out T> where T : IRecyclable<T>
    {
        event Action<T> OnRecycle;
    }
}