namespace Pancake
{
    public class GameComponent : UnityEngine.MonoBehaviour, IComponent
    {
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;
    }
}