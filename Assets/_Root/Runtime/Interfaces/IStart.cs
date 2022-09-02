namespace Pancake
{
    /// <summary>
    /// Defines a class that wants to receive a callback during the Start event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
    /// </summary>
    public interface IStart
    {
        void Start();
    }
}