namespace Pancake.Tween
{
    /// <summary>
    /// Extends the TweenAction API by Tween.
    /// </summary>
    public static class TweenActionExtensions
    {
        /// <summary>
        /// Plays the TweenAction.
        /// </summary>
        public static Tween Play(this TweenAction action) { return Tween.Create().Add(action).Play(); }


        /// <summary>
        /// Plays the delay TweenAction.
        /// </summary>
        public static Tween PlayDelay(this TweenAction action, float delay)
        {
            return Tween.Create()
                // delay in timeline, use one action
                .AddDelay(delay, action)
                // delay in action, use two actions
                //.AppendInterval(delay).Append(action)
                .Play();
        }
    }
}