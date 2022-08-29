using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UIQuery.Editor
{
    [CreateAssetMenu(menuName = "Pancake/UICopyTemplateSettings", fileName = "UICopyTemplateSettings")]
    public class UICopyTemplateSettings : ScriptableObject
    {
        public string ClassName => className;
        [SerializeField] private string className = DefaultClassName;
        public static readonly string DefaultClassName = "{0}UiElements";

        public string[] PickupComponentNames => pickupComponentNames;
        [SerializeField] private string[] pickupComponentNames = DefaultPickupComponentNames;
        public static readonly string[] DefaultPickupComponentNames = { nameof(Button), nameof(InputField), nameof(Text) };

        public string[] RemoveText => removeText;
        [SerializeField] private string[] removeText = DefaultRemoveText;
        public static readonly string[] DefaultRemoveText = { };
    }
}