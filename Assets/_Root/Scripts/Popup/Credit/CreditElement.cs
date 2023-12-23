using Pancake.Apex;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Credit Element")]
    public class CreditElement : ScriptableObject
    {
        public CreditElementType type;
        [ShowIf(nameof(type), CreditElementType.Message)] public string message;
        [ShowIf(nameof(type), CreditElementType.Image)] public Sprite sprite;
        [ShowIf(nameof(type), CreditElementType.Unit)] public string titleUnit;
        [ShowIf(nameof(type), CreditElementType.Unit)] [Array] public string[] unitMembers;
        [ShowIf(nameof(type), CreditElementType.Space)] public float space;
    }

    public enum CreditElementType
    {
        Space,
        Image,
        Message,
        Unit
    }
}