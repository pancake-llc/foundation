#if PANCAKE_ATOM
namespace UnityAtoms
{
    /// <summary>
    /// Interface for setting an event.
    /// </summary>
    public interface ISetEvent
    {
        void SetEvent<E>(E e) where E : AtomEventBase;
    }
}
#endif