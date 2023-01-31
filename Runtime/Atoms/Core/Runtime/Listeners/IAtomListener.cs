#if PANCAKE_ATOM
namespace UnityAtoms
{
    public interface IAtomListener
    {
        void OnEventRaised();
    }

    public interface IAtomListener<T>
    {
        void OnEventRaised(T item);
    }
}

#endif