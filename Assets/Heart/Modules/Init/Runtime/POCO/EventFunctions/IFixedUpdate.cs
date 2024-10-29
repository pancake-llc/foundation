using Sisus.Init.Internal;
using UnityEngine.Scripting;

namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that can <see cref="Updater.Subscribe(IFixedUpdate)">subscribe</see> to receive a callback during the FixedUpdate event function.
    /// </summary>
    [RequireImplementors]
    public interface IFixedUpdate : IEnableable
    {
        /// <summary>
        /// Frame-rate independent message for physics calculations.
        /// </summary>
        /// <param name="deltaTime"> Elapsed <see cref="UnityEngine.Time.fixedDeltaTime">time</see> in seconds since last Update event. </param>
        void FixedUpdate(float deltaTime);
    }
}