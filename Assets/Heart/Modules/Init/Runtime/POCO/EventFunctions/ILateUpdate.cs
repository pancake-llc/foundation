namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that can <see cref="Updater.Subscribe(ILateUpdate)">subscribe</see> to receive a callback during the LateUpdate event function.
    /// </summary>
    public interface ILateUpdate
    {
        /// <summary>
        /// <see cref="LateUpdate"/> is called every frame, if the <see cref="UnityEngine.MonoBehaviour">MonoBehaviour</see>
        /// is <see cref="UnityEngine.GameObject.activeInHierarchy">active</see> and <see cref="UnityEngine.Behaviour.enabled">enabled</see>.
        /// <para>
        /// <see cref="LateUpdate"/> is called after all <see cref="IUpdate.Update">Update</see> functions have been called. This is useful to order script execution.
        /// For example a follow camera should always be implemented in <see cref="LateUpdate"/> because it tracks objects that might have moved inside <see cref="IUpdate.Update">Update</para>.
        /// </para>
        /// </summary>
        /// <param name="deltaTime"> Elapsed <see cref="UnityEngine.Time.deltaTime">time</see> in seconds since last Update event. </param>
        void LateUpdate(float deltaTime);
    }
}