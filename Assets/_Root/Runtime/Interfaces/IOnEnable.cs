namespace Pancake
{
    /// <summary>
    /// Defines a class that wants to receive a callback during the OnEnable event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
    /// </summary>
    public interface IOnEnable
    {
        void OnEnable();
    }
}