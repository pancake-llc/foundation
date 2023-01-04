using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    internal static class SI_PropertyEditorUtility
    {
        internal static void Show(Object obj)
        {
            Type propertyEditor = typeof(EditorWindow).Assembly.GetTypes().FirstOrDefault(x => x.Name == "PropertyEditor");

            if (propertyEditor == null)
                return;

            MethodInfo openPropertyEditorMethod = propertyEditor.GetMethod("OpenPropertyEditor",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new Type[] {typeof(Object), typeof(bool)},
                null);

            openPropertyEditorMethod.Invoke(null, new object[] {obj, true});
        }
    }
}