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
        /// <summary>
        /// Each scene loaded by LoadSceneMode.Single
        /// </summary>
        SceneLoaded = 0,

        /// <summary>
        /// Each scene loaded by LoadSceneMode.Additive.</br>
        /// Use this option for compatibility with the use of LoadSceneMode.Additive instead of LoadSingle Scene introduced in foundation,
        /// to keep the variable's value reset behavior similar to SceneLoaded. </br>
        /// If you are not using a flow load scene like in foundation or you are not sure how to reset the value when the load scene is adaptive, do not use this option.
        /// </summary>
        AdditiveSceneLoaded = 2,
        ApplicationStarts = 1,
    }

    public enum StartupMode
    {
        Manual,
        Awake,
        Start,
        OnEnabled
    }

    public enum TargetFrameRate
    {
        ByDevice = -1,
        Frame60 = 60,
        Frame120 = 120,
        Frame240 = 240
    }
    
    public enum ENameAssetCreationMode
    {
        Auto,
        Manual
    }
}