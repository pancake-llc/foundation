using UnityEditor;
using UnityEditor.UIElements;

namespace Sisus.Shared.EditorOnly
{
    /// <summary>
    /// Extension methods for <see cref="SerializedProperty"/>.
    /// </summary>
    internal static class UIElementExtensions
    {
        internal static Editor GetEditor(this InspectorElement inspectorElement) => inspectorElement.editor;
    }
}