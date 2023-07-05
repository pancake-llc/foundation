using UnityEngine;

namespace Pancake
{
    [Searchable]
    [EditorIcon("scriptable_factory")]
    [CreateAssetMenu(fileName = "GameObjectFactory", menuName = "Pancake/Misc/Game Object Factory")]
    public class GameObjectFactory : ScriptableFactory<GameObject>
    {
        [SerializeField] private GameObject prefab;

        public override GameObject Create() { return Instantiate(prefab); }
    }
}