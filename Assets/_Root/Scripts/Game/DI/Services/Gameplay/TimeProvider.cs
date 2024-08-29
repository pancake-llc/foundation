using System;
using Sisus.Init;
using UnityEngine;
using static UnityEngine.Time;

namespace Pancake.Game
{
    /// <summary>
    /// Class responsible for providing information about the current time.
    /// </summary>
    [Service(typeof(ITimeProvider))]
    public class TimeProvider : ITimeProvider
    {
        /// <inheritdoc/>
        public float Time => time;

        /// <inheritdoc/>
        public float DeltaTime => deltaTime;

        /// <inheritdoc/>
        public float RealtimeSinceStartup => realtimeSinceStartup;

        /// <inheritdoc/>
        public DateTime Now => DateTime.UtcNow;

        /// <inheritdoc/>
        public object WaitForSeconds(float seconds) => new WaitForSeconds(seconds);
    }
}