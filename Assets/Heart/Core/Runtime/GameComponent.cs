using Pancake.Common;

namespace Pancake
{
    public class GameComponent : UnityEngine.MonoBehaviour, IComponent
    {
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;

        protected virtual void OnLoadComponents() { DebugEditor.LogWarning("Please override OnLoadComponents and implement the behavior you want for your settings!"); }

#if UNITY_EDITOR
        [Alchemy.Inspector.HorizontalLine(0.87f, 0.44f, 0.64f)]
        [Alchemy.Inspector.Button]
        private void LoadComponents()
        {
            OnLoadComponents();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}