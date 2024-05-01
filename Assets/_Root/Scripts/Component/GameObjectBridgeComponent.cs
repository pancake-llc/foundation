using Alchemy.Inspector;
using Pancake.Scriptable;

namespace Pancake.SceneFlow
{
    using UnityEngine;


    [EditorIcon("icon_default")]
    public class GameObjectBridgeComponent : GameComponent
    {
        [SerializeField, LabelText("Event")] private ScriptableEventGetGameObject getGameObjectEvent;
        [SerializeField] private GameObject target;

        protected void OnEnable() { getGameObjectEvent.OnRaised += GetGameObject; }

        protected void OnDisable() { getGameObjectEvent.OnRaised -= GetGameObject; }

        private GameObject GetGameObject() { return target; }
    }
}