using Pancake.Apex;
using Pancake.Scriptable;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [HideMonoScript]
    [EditorIcon("csharp")]
    public class GameObjectBridgeComponent : GameComponent
    {
        [SerializeField, Label("Event")] private ScriptableEventGetGameObject getGameObjectEvent;
        [SerializeField] private GameObject target;

        protected void OnEnable() { getGameObjectEvent.OnRaised += GetGameObject; }

        protected void OnDisable() { getGameObjectEvent.OnRaised -= GetGameObject; }

        private GameObject GetGameObject() { return target; }
    }
}