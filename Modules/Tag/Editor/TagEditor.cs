using UnityEditor;

namespace Pancake.TagEditor
{
    [CustomEditor(typeof(Tag.Tag), true)]
    [CanEditMultipleObjects]
    public class TagEditor : TagEditorBase
    {
    }
}