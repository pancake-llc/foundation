namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that can <see cref="Updater.Subscribe(IFixedUpdate)">subscribe</see> to receive a callback during the FixedUpdate event function.
    /// </summary>
    public interface IFixedUpdate
    {
        /// <summary>
        /// Frame-rate independent message for physics calculations.
        /// </summary>
        /// <param name="deltaTime"> Elapsed <see cref="UnityEngine.Time.fixedDeltaTime">time</see> in seconds since last Update event. </param>
        void FixedUpdate(float deltaTime);
    }
}