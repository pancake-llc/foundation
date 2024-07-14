namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that wants to receive a callback during the OnDisable event function of the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> that <see cref="Wrapper{TWrapped}">wraps</see> it.
    /// </summary>
    public interface IOnDisable
    {
        /// <summary>
        /// This function is called when the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see> becomes <see cref="UnityEngine.Behaviour.enabled">disabled</see>.
        /// <para>
        /// This is also called when the object is destroyed and can be used for any cleanup code. When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable after the script has been loaded.
        /// </para>
        /// </summary>
        void OnDisable();
    }
}