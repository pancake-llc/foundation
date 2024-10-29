using Sisus.Init.Internal;
using UnityEngine.Scripting;

namespace Sisus.Init
{
    /// <summary>
    /// Defines a class that can <see cref="Updater.Subscribe(IUpdate)">subscribe</see> to receive a callback during the Update event function.
    /// </summary>
    [RequireImplementors]
    public interface IUpdate : IEnableable
    {
        /// <summary>
        /// <see cref="Update"/> is called every frame.
        /// </summary>
        /// <param name="deltaTime"> Elapsed <see cref="UnityEngine.Time.deltaTime">time</see> in seconds since last Update event. </param>
        void Update(float deltaTime);
    }
}