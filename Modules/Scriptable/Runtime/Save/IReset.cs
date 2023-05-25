namespace Pancake.Scriptable
{
    public interface IReset
    {
        void ResetToInitialValue();
#if UNITY_EDITOR
        void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state);
#endif
    }
}