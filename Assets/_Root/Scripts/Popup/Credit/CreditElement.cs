using Alchemy.Inspector;
using UnityEngine.UIElements;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Pancake/Game/Credit Element")]
    public class CreditElement : ScriptableObject
    {
        public CreditElementType type;
#if UNITY_EDITOR
        private bool IsMessage => type == CreditElementType.Message;
        private bool IsImage => type == CreditElementType.Image;
        private bool IsUnit => type == CreditElementType.Unit;
        private bool IsSpace => type == CreditElementType.Space;
#endif
        [ShowIf("IsMessage")] public string message;
        [ShowIf("IsImage")] public Sprite sprite;
        [ShowIf("IsUnit")] public string titleUnit;

        [ShowIf("IsUnit")] [ListViewSettings(ShowAlternatingRowBackgrounds = AlternatingRowBackground.All, ShowFoldoutHeader = false)]
        public string[] unitMembers;

        [ShowIf("IsSpace")] public float space;
    }

    public enum CreditElementType
    {
        Space,
        Image,
        Message,
        Unit
    }
}