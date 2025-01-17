namespace Pancake.UI
{
    public enum EProgressBarStates
    {
        Idle,
        Decreasing,
        Increasing,
        InDecreasingDelay,
        InIncreasingDelay
    }

    /// the possible fill modes 
    public enum EFillModes
    {
        LocalScale,
        FillAmount,
        Width,
        Height,
        Anchor
    }

    /// the possible directions for the fill (for local scale and fill amount only)
    public enum EBarDirections
    {
        LeftToRight,
        RightToLeft,
        UpToDown,
        DownToUp
    }

    /// the possible ways to animate the bar fill
    public enum EBarFillModes
    {
        SpeedBased,
        FixedDuration
    }
}