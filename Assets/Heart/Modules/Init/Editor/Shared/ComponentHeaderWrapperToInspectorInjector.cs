//#define DEBUG_REPAINT
//#define DEBUG_ENABLED

using UnityEditor;
using UnityEngine;
using static Sisus.Shared.EditorOnly.InspectorContents;

namespace Sisus.Shared.EditorOnly
{
    [InitializeOnLoad]
    internal static class ComponentHeaderWrapperToInspectorInjector
    {
        static ComponentHeaderWrapperToInspectorInjector()
        {
            Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
            Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
        }

        private static void AfterInspectorRootEditorHeaderGUI(Editor rootEditor)
        {
            foreach((var componentEditor, var componentHeader) in GetAllHeaderElements(rootEditor))
            {
                if(IsMissingComponent(componentEditor))
                {
                    continue;
                }

                if(ComponentHeaderWrapper.IsWrapped(componentHeader))
                {
                    continue;
                }

#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(componentEditor.GetType().Name + ".Repaint");
				UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
#endif

                rootEditor.Repaint();

#if DEV_MODE && DEBUG_REPAINT
				UnityEngine.Profiling.Profiler.EndSample();
#endif

                if(Event.current.type != EventType.Repaint)
                {
                    return;
                }

#if DEV_MODE && DEBUG_ENABLED
				Debug.Log("WRAPPING: " + (componentEditor.target?.GetType().Name ?? "null") + " " + componentEditor.GetType().Name + " ("+componentEditor.GetInstanceID()+") " + componentHeader.onGUIHandler.Method.Name); // TEMP
#endif

                ComponentHeaderWrapper.Wrap(componentEditor, componentHeader, false);
            }

            bool IsMissingComponent(Editor componentEditor) => !componentEditor.target || componentEditor.target.GetType() == typeof(MonoBehaviour);
        }
    }
}