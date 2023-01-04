using UnityEditor;

namespace Pancake.Editor
{
    internal static class SI_SerializedPropertyExtensions
    {
        public static SerializedProperty ReferenceModeProperty(this SerializedProperty property) { return property.FindPropertyRelative("mode"); }

        public static SerializedProperty RawReferenceProperty(this SerializedProperty property) { return property.FindPropertyRelative("rawReference"); }

        public static SerializedProperty UnityReferenceProperty(this SerializedProperty property) { return property.FindPropertyRelative("unityReference"); }
    }
}