namespace Pancake.Scriptable
{
    public interface IReset
    {
        /// <summary>
        /// Interface for objects that can be reset to their initial value.
        /// </summary>
        void ResetToInitialValue();
#if UNITY_EDITOR
        void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state);
#endif
    }
}