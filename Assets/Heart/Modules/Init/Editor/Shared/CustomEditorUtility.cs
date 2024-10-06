using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;

namespace Sisus.Shared.EditorOnly
{
    public static class CustomEditorUtility
    {
        public static readonly Type GenericInspectorType = typeof(GenericInspector);

#if ODIN_INSPECTOR
        public static readonly Type OdinEditorType = typeof(Sirenix.OdinInspector.Editor.OdinEditor);
#endif

        public static bool IsDefaultOrOdinEditor([AllowNull] Type customEditorType) =>
#if ODIN_INSPECTOR
            customEditorType == OdinEditorType ||
#endif
            customEditorType == GenericInspectorType;
    }
}