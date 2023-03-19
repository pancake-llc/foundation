namespace Pancake
{
    using Attribute;
    using UnityEngine;

    [Searchable]
    [HideMono]
    [EditorIcon("scriptable_init")]
    [CreateAssetMenu(fileName = "Assets/_Root/Resources/InitProfile.asset", menuName = "Pancake/Misc/Profile Initialize")]
    public class InitProfile : ScriptableObject
    {
        [InfoBox("Has to be located in a Resources folder and named InitProfile")]
        public AutoInitialize[] collection;
    }
}