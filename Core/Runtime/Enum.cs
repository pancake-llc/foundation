namespace Pancake
{
    /// <summary>
    /// TimeMode
    /// </summary>
    public enum TimeMode
    {
        Normal = 0,
        Unscaled = 1,
    }

    /// <summary>
    /// UpdateMode
    /// </summary>
    public enum UpdateMode
    {
        Update = 0,
        LateUpdate = 1,
        FixedUpdate = 2,
        WaitForFixedUpdate = 3,
        WaitForEndOfFrame = 4
    }

    public enum ResetType
    {
        SceneLoaded,
        ApplicationStarts,
    }

    public enum StartupMode
    {
        Manual,
        Awake,
        Start,
        OnEnabled
    }
}