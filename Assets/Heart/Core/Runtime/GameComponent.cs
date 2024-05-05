using Pancake.Common;

namespace Pancake
{
    public class GameComponent : UnityEngine.MonoBehaviour, IComponent
    {
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;

        protected virtual void OnLoadComponents() { DebugEditor.LogWarning("Please override OnLoadComponents and implement the behavior you want for your settings!"); }
    }
}