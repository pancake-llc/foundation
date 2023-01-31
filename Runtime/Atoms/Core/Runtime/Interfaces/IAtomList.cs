#if PANCAKE_ATOM
namespace UnityAtoms
{
    /// <summary>
    /// Interface for Atom Lists.
    /// </summary>
    public interface IAtomList
    {
        void Add(AtomBaseVariable item);
        bool Remove(AtomBaseVariable item);
    }
}
#endif