namespace Pancake.Sound
{
    public interface IEffectDecoratable
    {
#if !UNITY_WEBGL
        internal IPlayerEffect AsDominator();
#endif
    }
}