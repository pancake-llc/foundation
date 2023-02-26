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
}