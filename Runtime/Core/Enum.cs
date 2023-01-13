using System.ComponentModel;

namespace Pancake
{
    public enum VariantType : sbyte
    {
        Object = -1,
        Null = 0,
        String = 1,
        Boolean = 2,
        Integer = 3,
        Float = 4,
        Double = 5,
        Vector2 = 6,
        Vector3 = 7,
        Vector4 = 8,
        Quaternion = 9,
        Color = 10,
        DateTime = 11,
        GameObject = 12,
        Component = 13,
        LayerMask = 14,
        Rect = 15,
        Numeric = 16,
        Ref = 17,
    }

    public enum QuitState
    {
        None,
        BeforeQuit,
        Quit
    }

    public enum EntityRelativity
    {
        Entity = 0,
        Self = 1,
        SelfAndChildren = 2
    }

    /// <summary>
    /// Search parameter type
    /// </summary>
    public enum SearchBy
    {
        Nothing = 0,
        Tag = 1,
        Name = 2,
        Type = 3
    }

    /// <summary>
    /// Describe an axis in cartesian coordinates, Useful for components that need to serialize which axis to use in some fashion.
    /// </summary>
    public enum CartesianAxis
    {
        Zneg = -3,
        Yneg = -2,
        Xneg = -1,
        X = 0,
        Y = 1,
        Z = 2
    }

    [System.Flags()]
    public enum ComparisonOperator
    {
        NotEqual = 0,
        LessThan = 1,
        GreaterThan = 2,
        NotEqualAlt = 3,
        Equal = 4,
        [Description("Less Than Or Equal")] LessThanEqual = 5,
        [Description("Greater Than Or Equal")] GreatThanEqual = 6,
        Always = 7
    }

    public enum AudioInterruptMode
    {
        StopIfPlaying = 0,
        DoNotPlayIfPlaying = 1,
        PlayOverExisting = 2
    }

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

    public enum ButtonSize
    {
        Small = 0,
        Medium = 22,
        Large = 32,
        Gigantic = 62,
    }

    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    public enum RotationSpace
    {
        /// <summary>An intrinsic rotation around its own local axes, usually called "local" or "self" space. Equivalent to <c>q*rotation</c></summary>
        Self,

        /// <summary>Rotation around its pre-rotation axes, usually "world" space. Equivalent to <c>rotation*q</c></summary>
        Extrinsic
    }
    
    public enum AudioState
    {
        Stop = 0,
        Playing = 1,
        Pause = 2,
        AwaitPlaying = 3,
        AwaitPause = 4,
        AwaitStop = 5,
    }

    public enum SoundLoop
    {
        None,
        Loop,
        Number
    }
}