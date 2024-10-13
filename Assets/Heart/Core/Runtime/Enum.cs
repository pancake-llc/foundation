namespace Pancake
{
    public enum EStartupMode
    {
        Manual,
        Awake,
        Start,
        OnEnabled
    }

    public enum ETargetFrameRate
    {
        ByDevice = -1,
        Frame60 = 60,
        Frame120 = 120,
        Frame240 = 240
    }

    public enum ECreationMode
    {
        Auto,
        Manual
    }

    [System.Flags]
    public enum EGameLoopType
    {
        None = 0,
        Update = 1 << 0,
        FixedUpdate = 1 << 1,
        LateUpdate = 1 << 2
    }

    public enum EAlignment
    {
        Left,
        Right,
        Top,
        Bottom,
        Center
    }

    public enum EFourDirection
    {
        Left,
        Right,
        Top,
        Down
    }
}