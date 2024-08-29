using System;

namespace Pancake
{
    /// <summary>
    /// Represents an object responsible for providing information about the current time.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// The time at the beginning of this frame (Read Only).
        /// <para>
        /// This is the time in seconds since the start of the application, which Time.timeScale scales and Time.maximumDeltaTime adjusts. When called from inside MonoBehaviour.FixedUpdate, it returns Time.fixedTime.
        /// </para>
        /// <para>
        /// This value is undefined during Awake messages and starts after all of these messages are finished. This value does not update if the Editor is paused. See Time.realtimeSinceStartup for a time value that is unaffected by pausing.
        /// </para>
        /// </summary>
        float Time { get; }

        /// <summary>
        /// The interval in seconds from the last frame to the current one (Read Only).
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// The real time in seconds since the game started (Read Only).
        /// <para>
        /// This is the time in seconds since the start of the application, and is not constant if called multiple times in a frame. Time.timeScale does not affect this property.
        /// </para>
        /// <para>
        /// In almost all cases you should use <see cref="UnityEngine.Time.time"/> or unscaledTime instead.
        /// </para>
        /// <para>
        /// Using realtimeSinceStartup is useful if you want to set Time.timeScale to zero to pause your application, but still want to be able to measure time somehow. In Editor scripts, you can also use realtimeSinceStartup to measure time while the Editor is paused.
        /// </para>
        /// <para>
        /// realtimeSinceStartup returns time as reported by the system timer. Depending on the platform and the hardware, it might report the same time even in several consecutive frames. If you're dividing something by time difference, take this into account (for example, time difference might become zero).
        /// </para>
        /// </summary>
        float RealtimeSinceStartup { get; }

        /// <summary>
        /// Gets a DateTime object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).
        /// <para>
        /// If you need to get the user's local time, for example for displaying it to the user, call <see cref="DateTime.ToLocalTime"/> on the
        /// value returned by this property.
        /// </para>
        /// </summary>
        DateTime Now { get; }

        /// <summary>
        /// Returns a yield instruction for waiting the given amount of seconds.
        /// </summary>
        /// <param name="seconds">
        /// Returned yield instruction will suspend coroutine execution for
        /// the given amount of seconds using scaled time.
        /// </param>
        /// <returns> A yield instruction. </returns>
        object WaitForSeconds(float seconds);
    }
}