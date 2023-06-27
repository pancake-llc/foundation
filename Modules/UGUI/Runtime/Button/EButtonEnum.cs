namespace Pancake.UI
{
    public enum EButtonClickType
    {
        /// <summary>
        /// Only executed single click.
        /// </summary>
        OnlySingleClick = 0,

        /// <summary>
        /// Only executed double click
        /// </summary>
        OnlyDoubleClick = 1,

        /// <summary>
        /// (single click or long click)
        /// Execute button onClick event after a period of time
        /// If onLongClick is already called then onClick will not be called again when you release your hand
        /// remove double click executed
        /// </summary>
        LongClick = 2,

        /// <summary>
        /// Normal click type (single click + double click)
        /// single click will get executed before a double click (dual actions)
        /// </summary>
        Instant = 3,

        /// <summary>
        /// If it's a double click, the single click will not executed
        /// use this if you want to make sure single click not execute before a double click
        /// the downside is that there is a delay when executing the single click (the delay is the double click register interval)
        /// </summary>
        Delayed = 4,

        /// <summary>
        /// (single click or hold)
        /// Execute button onClick event as normal
        /// If hold is already called then onClick will not be called again when you release your hand
        /// remove double click executed
        /// </summary>
        Hold = 5
    }

    public enum EButtonMotion
    {
        /// <summary>
        /// No movement, value is changed instantly
        /// </summary>
        Immediate = 0,

        /// <summary>
        /// Motion is separated between Down and Up
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Continuous motion as soon as the mouse is pressed down and automatically ends the cycle, no movement when releasing the mouse
        /// </summary>
        Uniform = 2,

        /// <summary>
        /// Continuous motion as soon as the mouse is released and automatically ends the cycle, no motion when pressing down at first
        /// </summary>
        Late = 3,
    }
}