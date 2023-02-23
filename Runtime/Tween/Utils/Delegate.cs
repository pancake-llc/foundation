namespace Pancake.Tween
{
    public delegate float EaseDelegate(float a, float b, float t);

    /// <summary>
    /// Used for tween callbacks
    /// </summary>
    public delegate void TweenCallback();

    /// <summary>
    /// Used for tween callbacks
    /// </summary>
    public delegate void TweenCallback<in T>(T value);
}