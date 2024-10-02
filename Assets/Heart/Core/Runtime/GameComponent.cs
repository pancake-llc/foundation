using Pancake.Common;
using Sirenix.OdinInspector;

namespace Pancake
{
    public class GameComponent : SerializedMonoBehaviour, IComponent
    {
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;
    }
}