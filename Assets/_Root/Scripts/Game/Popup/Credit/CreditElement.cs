using Sirenix.OdinInspector;

namespace Pancake.Game.UI
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Credit Element")]
    public class CreditElement : ScriptableObject
    {
        public CreditElementType type;
        [ShowIf("IsMessage")] public string message;
        [ShowIf("IsImage")] public Sprite sprite;
        [ShowIf("IsUnit")] public string titleUnit;
        [ShowIf("IsUnit")] public string[] unitMembers;
        [ShowIf("IsSpace")] public float space;

#if UNITY_EDITOR
        private bool IsMessage => type == CreditElementType.Message;
        private bool IsImage => type == CreditElementType.Image;
        private bool IsUnit => type == CreditElementType.Unit;
        private bool IsSpace => type == CreditElementType.Space;
#endif
    }

    public enum CreditElementType
    {
        Space,
        Image,
        Message,
        Unit
    }
}