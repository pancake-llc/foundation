namespace Pancake.PlayerLoop
{
    public enum PlayerLoopTiming
    {
        PreTimeUpdate = 0,
        PostTimeUpdate = 1,

        PreInitialization = 2,
        PostInitialization = 3,

        PreStart = 4,
        PostStart = 5,

        PreFixedUpdate = 6,
        PostFixedUpdate = 7,

        PreUpdate = 8,
        PostUpdate = 9,

        PreLateUpdate = 10,
        PostLateUpdate = 11,
    }
}